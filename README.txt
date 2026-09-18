Xbox Controller Toggle — versión pulida
=============================================

Dispositivo configurado:
USB\VID_045E&PID_02D1

Uso:
1. Ejecuta XboxControllerToggle.exe.
2. Windows mostrará UAC UNA VEZ al iniciar porque la aplicación necesita
   privilegios de administrador para administrar el dispositivo.
3. El programa queda en la bandeja del sistema.
4. Clic izquierdo: alterna Xbox ON/OFF.
5. Clic derecho: Activar, Desactivar, Actualizar estado o Salir.

No se desconecta eléctricamente el USB. Windows deshabilita/habilita la
instancia PnP del mando.

Compilación en Windows:
dotnet publish -c Release

EXE:
bin\Release\net8.0-windows\win-x64\publish\XboxControllerToggle.exe

Nota:
La identificación utiliza VID_045E&PID_02D1. Si Windows muestra varias
instancias con ese VID/PID, el programa selecciona la primera encontrada.


COMPILACIÓN AUTOMÁTICA CON GITHUB ACTIONS
=========================================

1. Sube TODOS los archivos y carpetas de este ZIP a un repositorio de GitHub.
2. En GitHub abre la pestaña "Actions".
3. Selecciona "Build Xbox Controller Toggle".
4. Pulsa "Run workflow".
5. Espera a que termine el trabajo.
6. Abre la ejecución terminada y busca "Artifacts".
7. Descarga "XboxControllerToggle-win-x64".
8. Dentro del ZIP estará XboxControllerToggle.exe.

No hace falta instalar Visual Studio ni .NET en tu PC.
