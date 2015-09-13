@echo /nologo
@if defined TYPE (
  @echo %TYPE%
)
@echo /optimize+
@echo /lib:"%REIPATCHER_DIR%" /r:ReiPatcher.exe /r:mono.cecil.dll /r:mono.cecil.rocks.dll
@echo /lib:"%UNITY_INJECTOR_DIR%" /r:UnityInjector.dll
@echo /lib:"%CM3D2_MOD_MANAGED_DIR%" /r:UnityEngine.dll /r:Assembly-CSharp.dll /r:Assembly-CSharp-firstpass.dll
@if defined OPTS (
  @echo %OPTS%
)
@echo /out:"%OUT%"
@echo %SRCS%
