#pragma once

class Cm3d2Dll {
public:
    Cm3d2Dll(const char* dllPath) {
        loadLibrary(dllPath);
    }

    ~Cm3d2Dll() {
        unloadLibrary();
    }

    void CreateFileSystemArchive() {
        return dll.FileSystem_CreateFileSystemArchive(&fsdata);
    }

    void DeleteFileSystem() {
        return dll.FileSystem_DeleteFileSystem(&fsdata);
    }

    void AddArchive(const char* path) {
        return dll.FileSystem_AddArchive(&fsdata, (char*) path);
    }

    std::vector<std::string> GetFiles() {
        std::vector<std::string> filenames;
        LISTDATA listdata {};
        dll.FileSystem_CreateList(&fsdata, "", 3, &listdata);
        filenames.resize(listdata.size);
        for(size_t i = 0; i < listdata.size; ++i) {
            std::array<char,1024> buf {};
            const auto r = dll.FileSystem_AtList(&listdata, (int) i, buf.data(), (int) buf.size());
            filenames[i] = buf.data();
        }
        dll.FileSystem_DeleteList(&listdata);
        return filenames;
    }

    std::vector<char> GetFile(const char* path) {
        // DLL側がマルチスレッドに対応していないので、ロックが必要
        std::unique_lock<std::mutex> lock(mtx);
        std::vector<char> file;
        FILEDATA filedata {};
        dll.FileSystem_GetFile(&fsdata, path, &filedata);
        file.resize(dll.File_GetSize(&filedata));
        dll.File_Read(&filedata, file.data(), file.size());
        dll.File_CloseFile(&filedata);
        return file;
    }

private:
    void loadLibrary(const char* dllPath) {
        unloadLibrary();
        hLib = LoadLibraryA(dllPath);
        if(hLib == nullptr) {
            return;
        }
        dll.set([&](const char* procName) {
            return GetProcAddress(hLib, procName);
        });
    }

    void unloadLibrary() {
        if(hLib) {
            FreeLibrary(hLib);
            hLib = nullptr;
        }
    }

    struct FSDATA {
        void*   ptr;
        int32_t type;
    };

    struct LISTDATA {
        int32_t size;
        void* data;
    };

    struct FILEDATA {
        void* object_pointer;
    };

    struct {
        using GetProcAddressFunc = std::function<void*(const char*)>;

        void set(const GetProcAddressFunc& getProcAddress) {
            *(void**)&FileSystem_CreateFileSystemArchive   = getProcAddress("DLL_FileSystem_CreateFileSystemArchive");
            *(void**)&FileSystem_DeleteFileSystem          = getProcAddress("DLL_FileSystem_DeleteFileSystem");
            *(void**)&FileSystem_AddArchive                = getProcAddress("DLL_FileSystem_AddArchive");
            *(void**)&FileSystem_CreateList                = getProcAddress("DLL_FileSystem_CreateList");
            *(void**)&FileSystem_DeleteList                = getProcAddress("DLL_FileSystem_DeleteList");
            *(void**)&FileSystem_AtList                    = getProcAddress("DLL_FileSystem_AtList");
            *(void**)&FileSystem_GetFile                   = getProcAddress("DLL_FileSystem_GetFile");
            *(void**)&File_IsValid                         = getProcAddress("DLL_File_IsValid");
            *(void**)&File_Read                            = getProcAddress("DLL_File_Read");
            *(void**)&File_GetSize                         = getProcAddress("DLL_File_GetSize");
            *(void**)&File_CloseFile                       = getProcAddress("DLL_File_CloseFile");
        }

        using DLL_FileSystem_CreateFileSystemArchive    = void (FSDATA*);
        using DLL_FileSystem_DeleteFileSystem           = void (FSDATA*);
        using DLL_FileSystem_AddArchive                 = void (FSDATA*, const char*);
        using DLL_FileSystem_CreateList                 = void (FSDATA*, const char*, int, LISTDATA*);
        using DLL_FileSystem_DeleteList                 = void (LISTDATA*);
        using DLL_FileSystem_AtList                     = int (LISTDATA*, int, char*, int);
        using DLL_FileSystem_GetFile                    = int (FSDATA*, const char*, FILEDATA*);
        using DLL_File_IsValid                          = int (FILEDATA*);
        using DLL_File_Read                             = int64_t (FILEDATA*, char*, int64_t);
        using DLL_File_GetSize                          = int64_t (FILEDATA*);
        using DLL_File_CloseFile                        = void (FILEDATA*);

        DLL_FileSystem_CreateFileSystemArchive*     FileSystem_CreateFileSystemArchive;
        DLL_FileSystem_DeleteFileSystem*            FileSystem_DeleteFileSystem;
        DLL_FileSystem_AddArchive*                  FileSystem_AddArchive;
        DLL_FileSystem_CreateList*                  FileSystem_CreateList;
        DLL_FileSystem_DeleteList*                  FileSystem_DeleteList;
        DLL_FileSystem_AtList*                      FileSystem_AtList;
        DLL_FileSystem_GetFile*                     FileSystem_GetFile;
        DLL_File_IsValid*                           File_IsValid;
        DLL_File_Read*                              File_Read;
        DLL_File_GetSize*                           File_GetSize;
        DLL_File_CloseFile*                         File_CloseFile;
    } dll;

    HMODULE hLib {};
    FSDATA fsdata {};
    mutable std::mutex mtx;
};
