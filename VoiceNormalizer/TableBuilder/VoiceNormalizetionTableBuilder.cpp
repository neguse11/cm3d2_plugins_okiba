/*
コンパイルには Visual C++ 2015 が必要です (Visual Studio Community 2015 内の Visual C++ など)

コンパイルと実行 (x64)
    call "%VS140COMNTOOLS%..\..\VC\"vcvarsall.bat amd64
    cl /nologo /EHsc VoiceNormalizetionTableBuilder.cpp
    .\VoiceNormalizetionTableBuilder.exe > VoiceNormalizerTable.txt

コンパイルと実行 (x86)
    call "%VS140COMNTOOLS%"vsvars32.bat
    cl /nologo /EHsc VoiceNormalizetionTableBuilder.cpp
    .\VoiceNormalizetionTableBuilder.exe > VoiceNormalizerTable.txt

不明な点

 - いくつかの例外となるファイルが存在する
    - 「s2_00434a.ogg」は通常の命名規則「xN_NNNNN.ogg」に当てはまらない例外ファイル名。
      スクリプトからは呼び出されていないようなので、とりあえず無視
    - いくつかのファイル（例：s0_06323）は voice_X と voice_2 の両方に存在する
        - todo どちらが優先されているのか調べること
    - いくつかのファイル（例：s0_05004）は voice_X と voice/edit の両方に存在する
        - todo どちらが優先されているのか調べること
*/
#define _CRT_SECURE_NO_WARNINGS
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <stdint.h>
#include <array>
#include <future>
#include <algorithm>
#include <map>
#include <string>
#include <functional>
#include <regex>
#pragma warning(push)
#pragma warning(disable: 4244)
#include "stb_vorbis.c" // https://github.com/nothings/stb/blob/master/stb_vorbis.c
#pragma warning(pop)
#include "cm3d2dll.hpp"


///////////////////////////////////////////////////////////////
using WalkFunc = std::function<void(const std::string& path, const std::string& filename)>;
using FilterFunc = std::function<void(const short* decoded, int len, int channels)>;


std::string getInstallPath() {
    std::string s;
    {
        std::array<char,1024> buf {};
        HKEY hKey {};
        RegOpenKeyA(HKEY_CURRENT_USER, "Software\\KISS\\カスタムメイド3D2", &hKey);
        if(hKey) {
            DWORD size = (DWORD) buf.size();
            const auto q = RegQueryValueExA(hKey, "InstallPath", 0, nullptr, (LPBYTE)buf.data(), &size);
            RegCloseKey(hKey);
        }
        s = buf.data();
    }
    if(! s.empty() && s.back() != '\\') {
        s.push_back('\\');
    }
    return s;
}


void walkDir(std::string path, const WalkFunc& walkFunc) {
    WIN32_FIND_DATAA wfd {};
    HANDLE h = FindFirstFileA((path + "\\*.*").c_str(), &wfd);
    if(h != INVALID_HANDLE_VALUE) {
        do {
            if(wfd.cFileName[0] != '.') {
                if((wfd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0) {
                    walkDir(path + "\\" + wfd.cFileName, walkFunc);
                } else {
                    walkFunc(path, wfd.cFileName);
                }
            }
        } while(FindNextFileA(h, &wfd));
        FindClose(h);
    }
}


void filterOggFile(const std::vector<char>& file, const FilterFunc& filterFunc) {
    int result = 0;
    int channels = 0;
    int sample_rate = 0;
    short* decoded = nullptr;
    int len = stb_vorbis_decode_memory((const unsigned char*)file.data(), (int)file.size(), &channels, &sample_rate, &decoded);
    if(decoded && len > 0) {
        filterFunc(decoded, len, channels);
    }
    free(decoded);
}


///////////////////////////////////////////////////////////////
struct ResultValues {
    int     peak;
    float   rms;
};

using Results = std::map<std::string, ResultValues>;

void dumpArc(Cm3d2Dll* dll, const std::string& arcPath, Results& results) {
    dll->CreateFileSystemArchive();
    dll->AddArchive(arcPath.c_str());

    std::vector<std::string> oggFilenames;
    const std::regex oggFilenamePattern(R"(([A-Za-z]\d_\d\d\d\d\d)\.[Oo][Gg][Gg])");
    for(const auto& filename : dll->GetFiles()) {
        char fname[_MAX_FNAME], ext[_MAX_EXT];
        _splitpath(filename.c_str(), nullptr, nullptr, fname, ext);
        strcat(fname, ext);
        if(std::regex_match(fname, oggFilenamePattern)) {
            oggFilenames.push_back(filename);
        }
    }

    // oggファイルの処理 (マルチスレッド動作)
    std::vector<std::future<ResultValues>> futures(oggFilenames.size());
    for(size_t i = 0; i < futures.size(); ++i) {
        futures[i] = std::async([&](size_t i) {
            ResultValues resultValues {};
            filterOggFile(
                dll->GetFile(oggFilenames[i].c_str()),
                [&](const short* decoded, int len, int channels) {
                    // peak
                    {
                        int vmin = 0;
                        int vmax = 0;
                        for(int i = 0; i < len * channels; ++i) {
                            short d = decoded[i];
                            if(d < vmin) vmin = d;
                            else if(d > vmax) vmax = d;
                        }
                        if(-vmin > vmax) resultValues.peak = -vmin;
                        else resultValues.peak = vmax;
                    }
                    // RMS
                    {
                        // 50ms で 1% に減るローパスフィルタ
                        const float samplingRate = 44100.0f;
                        const float milliSeconds = 1.0f / 1000.0f;
                        const float coef = exp(log(0.01f) / (50.0f * milliSeconds * samplingRate));
                        float yMax = 0.0f;
                        float y1 = 0.0f;
                        for(int i = 0, n = len * channels; i < n; ++i) {
                            short d = decoded[i];
                            float f = (float)d * (1.0f / 32768.0f);
                            f *= f;
                            y1 = f + coef * (y1 - f);
                            if(y1 > yMax) {
                                yMax = y1;
                            }
                        }
                        resultValues.rms = sqrtf(yMax);
                    }
                });
            return resultValues;
        }, i);
    }

    // 処理結果を格納
    for(size_t i = 0; i < futures.size(); ++i) {
        char fname[256], ext[256];
        _splitpath(oggFilenames[i].c_str(), nullptr, nullptr, fname, ext);
        const auto r = futures[i].get();
        if(i % 50 == 0) {
            fprintf(stderr, "(%6zd/%6zd) %s = %04x\r", i, futures.size(), fname, r.peak);
        }
        _strupr(fname);
        results[fname] = r;
    }
    dll->DeleteFileSystem();
}


void dumpAll(const std::string& dllPath, const std::string& basePath, FILE* fp) {
    Results results;

    // *.arc を探して dumpArc を実行
    const std::regex filenamePattern(R"(.*\.[Aa][Rr][Cc])");
    std::unique_ptr<Cm3d2Dll> dll = std::make_unique<Cm3d2Dll>(dllPath.c_str());
    walkDir(basePath, [&](const std::string& path, const std::string& filename) {
        if(std::regex_match(filename, filenamePattern)) {
            dumpArc(dll.get(), path + "\\" + filename, results);
        }
    });

    // 結果を出力
    for(auto it = results.cbegin(); it != results.cend(); ++it) {
        const ResultValues& resultValues = it->second;
        fprintf(fp, "%s,%d,%f\n", it->first.c_str(), resultValues.peak, resultValues.rms);
    }
}


int main(int argc, const char** argv) {
    std::string outFilename;
    if(argc > 1) {
        outFilename = argv[1];
    }

    const std::string installPath = getInstallPath();
    if(installPath.empty()) {
        fprintf(stderr, "インストール情報がありません\n");
        return 1;
    }

    const std::string basePath = installPath + "GameData";

#if defined(_WIN64)
    const std::string dllPath = installPath + "CM3D2x64_Data\\Plugins\\cm3d2_x64.dll";
#else
    const std::string dllPath = installPath + "CM3D2x86_Data\\Plugins\\cm3d2_x86.dll";
#endif

    clock_t cStart = clock();
    FILE* fp = stdout;
    if(! outFilename.empty()) {
        fp = fopen(outFilename.c_str(), "wt");
    }
    dumpAll(dllPath, basePath, fp);
    if(fp != stdout) {
        fclose(fp);
    }
    fprintf(stderr, "\n%6.2f sec\n", (double)(clock() - cStart) / CLOCKS_PER_SEC);
}
