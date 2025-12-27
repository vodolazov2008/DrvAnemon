# ПЛАН ПЕРЕИМЕНОВАНИЯ DRVANEMON → DRVANEMON3

## Цель
Переименовать проект и его зависимости в DrvAnemon3 для возможности одновременного использования с оригинальным DrvAnemon.

## План выполнения:

### 1. ПОДГОТОВКА
- [ ] Создать резервную копию проекта
- [ ] Создать план выполнения

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

### 4. ОБНОВЛЕНИЕ SOLUTION ФАЙЛА
- [x] DrvAnemon3.sln:
  - Имена проектов: DrvAnemon → DrvAnemon3
  - Пути к проектам
  - Новые GUID для проектов
  - SolutionGuid

### 5. ОБНОВЛЕНИЕ ИСХОДНОГО КОДА
- [ ] DriverUtils.cs:
  - DriverCode: "DrvAnemon" → "DrvAnemon3"
  - Namespace: Scada.Comm.Drivers.DrvAnemon → Scada.Comm.Drivers.DrvAnemon3
- [ ] DrvAnemonLogic.cs:
  - Code: "DrvAnemon" → "DrvAnemon3"
  - Namespace: Scada.Comm.Drivers.DrvAnemon → Scada.Comm.Drivers.DrvAnemon3
- [ ] Все остальные файлы в DrvAnemon3.Logic:
  - Namespace: Scada.Comm.Drivers.DrvAnemon → Scada.Comm.Drivers.DrvAnemon3
- [ ] Все файлы в DrvAnemon3.View:
  - Namespace: Scada.Comm.Drivers.DrvAnemon → Scada.Comm.Drivers.DrvAnemon3

### 6. ОЧИСТКА И ВОССТАНОВЛЕНИЕ ПРОЕКТА
- [ ] Удаление obj и bin директорий
- [ ] Восстановление NuGet пакетов
- [ ] Сборка проекта для проверки

### 7. ФИНАЛЬНАЯ ПРОВЕРКА
- [ ] Проверка корректности переименования
- [ ] Тестирование сборки
- [ ] Обновление документации

## Заметки:
- Новый код драйвера: "DrvAnemon3"
- Новая версия: 3.0.0.0
- Новые namespace: Scada.Comm.Drivers.DrvAnemon3
