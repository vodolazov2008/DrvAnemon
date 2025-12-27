# ПЛАН ПЕРЕИМЕНОВАНИЯ DRVANEMON → DRVANEMON3

## Цель
Переименовать проект и его зависимости в DrvAnemon3 для возможности одновременного использования с оригинальным DrvAnemon.

## План выполнения:

### 1. ПОДГОТОВКА
- [x] Создать резервную копию проекта
- [x] Создать план выполнения

### 2. ПЕРЕИМЕНОВАНИЕ ДИРЕКТОРИЙ
- [x] DrvAnemon.Logic → DrvAnemon3.Logic
- [x] DrvAnemon.View → DrvAnemon3.View
- [x] DrvAnemon.sln → DrvAnemon3.sln

### 3. ОБНОВЛЕНИЕ .CSPROJ ФАЙЛОВ
- [x] DrvAnemon3.Logic/DrvAnemon3.Logic.csproj:
  - AssemblyName: DrvAnemon.Logic → DrvAnemon3.Logic
  - RootNamespace: Scada.Comm.Drivers.DrvAnemon → Scada.Comm.Drivers.DrvAnemon3
  - AssemblyVersion: 6.0.0.0 → 3.0.0.0
  - FileVersion: 6.0.0.0 → 3.0.0.0
- [x] DrvAnemon3.View/DrvAnemon3.View.csproj:
  - AssemblyName: DrvAnemon.View → DrvAnemon3.View
  - RootNamespace: Scada.Comm.Drivers.DrvAnemon → Scada.Comm.Drivers.DrvAnemon3
  - AssemblyVersion: 6.0.0.0 → 3.0.0.0
  - FileVersion: 6.0.0.0 → 3.0.0.0
  - Добавлены ссылки на ScadaComm сборки

### 4. ОБНОВЛЕНИЕ SOLUTION ФАЙЛА
- [x] DrvAnemon3.sln:
  - Имена проектов: DrvAnemon → DrvAnemon3
  - Пути к проектам
  - Новые GUID для проектов
  - SolutionGuid

### 5. ОБНОВЛЕНИЕ ИСХОДНОГО КОДА
- [x] DriverUtils.cs (в обоих проектах):
  - DriverCode: "DrvAnemon" → "DrvAnemon3"
  - Namespace: Scada.Comm.Drivers.DrvAnemon → Scada.Comm.Drivers.DrvAnemon3
- [x] DrvAnemonLogic.cs:
  - Code: "DrvAnemon" → "DrvAnemon3"
  - Namespace: Scada.Comm.Drivers.DrvAnemon → Scada.Comm.Drivers.DrvAnemon3
- [x] Все остальные файлы в DrvAnemon3.Logic:
  - Namespace: Scada.Comm.Drivers.DrvAnemon → Scada.Comm.Drivers.DrvAnemon3
- [x] Все файлы в DrvAnemon3.View:
  - Namespace: Scada.Comm.Drivers.DrvAnemon → Scada.Comm.Drivers.DrvAnemon3
- [x] AssemblyInfo.cs файлы обновлены
- [x] Test файл обновлен

### 6. ОЧИСТКА И ВОССТАНОВЛЕНИЕ ПРОЕКТА
- [x] Удаление obj и bin директорий
- [x] Восстановление NuGet пакетов
- [x] Сборка проекта для проверки

### 7. ФИНАЛЬНАЯ ПРОВЕРКА
- [x] Проверка корректности переименования
- [x] Тестирование сборки (успешно!)
- [x] Обновление документации

## Результат
Проект успешно переименован в DrvAnemon3 и готов к одновременному использованию с оригинальным DrvAnemon.

## Заметки:
- Новый код драйвера: "DrvAnemon3"
- Новая версия: 3.0.0.0
- Новые namespace: Scada.Comm.Drivers.DrvAnemon3
- Сборки: DrvAnemon3.Logic.dll и DrvAnemon3.View.dll

