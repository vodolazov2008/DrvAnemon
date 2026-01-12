# Makefile для DrvAnemon проекта
# Автоматизация сборки .NET проектов Rapid SCADA драйвера

# Переменные
SOLUTION = DrvAnemon3.sln
LOGIC_PROJECT = DrvAnemon3.Logic/DrvAnemon3.Logic.csproj
VIEW_PROJECT = DrvAnemon3.View/DrvAnemon3.View.csproj
BUILD_CONFIG = Release
DOTNET = dotnet

# Цели по умолчанию
.PHONY: all build clean test help install restore

# Сборка только основного проекта Logic
all: restore build-logic

# Полная сборка решения
build: $(SOLUTION)
	@echo "🚀 Сборка проекта DrvAnemon..."
	$(DOTNET) build $(SOLUTION) --configuration $(BUILD_CONFIG) --verbosity normal
	@echo "✅ Сборка завершена успешно!"

# Сборка только логики
build-logic: $(LOGIC_PROJECT)
	@echo "🧠 Сборка DrvAnemon.Logic..."
	$(DOTNET) build $(LOGIC_PROJECT) --configuration $(BUILD_CONFIG) --verbosity normal
	@echo "✅ Логика собрана успешно!"

# Сборка только представления
build-view: $(VIEW_PROJECT)
	@echo "🖼️ Сборка DrvAnemon.View..."
	$(DOTNET) build $(VIEW_PROJECT) --configuration $(BUILD_CONFIG) --verbosity normal
	@echo "✅ Представление собрано успешно!"

# Восстановление зависимостей
restore:
	@echo "📦 Восстановление зависимостей..."
	$(DOTNET) restore $(SOLUTION)
	@echo "✅ Зависимости восстановлены!"

# Очистка сборок
clean:
	@echo "🧹 Очистка проекта..."
	$(DOTNET) clean $(SOLUTION) --configuration $(BUILD_CONFIG)
	@echo "🗑️ Удаление bin и obj папок..."
	rm -rf */bin */obj
	@echo "✅ Очистка завершена!"

# Запуск тестов (если будут добавлены)
test:
	@echo "🧪 Запуск тестов..."
	$(DOTNET) test $(SOLUTION) --configuration $(BUILD_CONFIG) --verbosity normal
	@echo "✅ Тесты завершены!"

# Сборка для публикации
publish:
	@echo "📤 Сборка для публикации..."
	$(DOTNET) publish $(LOGIC_PROJECT) --configuration $(BUILD_CONFIG) --output ./publish/Logic
	$(DOTNET) publish $(VIEW_PROJECT) --configuration $(BUILD_CONFIG) --output ./publish/View
	@echo "✅ Публикация завершена! Результат в папке ./publish/"

# Проверка кода
lint:
	@echo "🔍 Проверка кода..."
	@echo "⚠️ Проверка зависимостей..."
	$(DOTNET) list package --outdated
	@echo "✅ Проверка завершена!"

# Быстрая пересборка
rebuild: clean build-logic

# Информация о проекте
info:
	@echo "📊 Информация о проекте DrvAnemon:"
	@echo "  Решение: $(SOLUTION)"
	@echo "  Конфигурация: $(BUILD_CONFIG)"
	@echo "  .NET SDK версия:"
	$(DOTNET) --version
	@echo "  Проекты в решении:"
	@$(DOTNET) sln $(SOLUTION) list

# Проверка работоспособности
check: restore
	@echo "🔧 Проверка работоспособности Logic проекта..."
	$(DOTNET) build $(LOGIC_PROJECT) --configuration $(BUILD_CONFIG) --verbosity minimal
	@echo "✅ DrvAnemon.Logic собирается без ошибок!"

# Помощь
help:
	@echo "🏗️ Makefile для DrvAnemon проекта"
	@echo ""
	@echo "Доступные цели:"
	@echo "  all        - Полная сборка (по умолчанию)"
	@echo "  build      - Сборка всего решения"
	@echo "  build-logic- Сборка только DrvAnemon.Logic"
	@echo "  build-view - Сборка только DrvAnemon.View"
	@echo "  restore    - Восстановление зависимостей"
	@echo "  clean      - Очистка проекта"
	@echo "  test       - Запуск тестов"
	@echo "  publish    - Сборка для публикации"
	@echo "  lint       - Проверка кода"
	@echo "  rebuild    - Полная пересборка"
	@echo "  info       - Информация о проекте"
	@echo "  check      - Проверка работоспособности"
	@echo "  help       - Показать эту справку"
	@echo ""
	@echo "Примеры использования:"
	@echo "  make          # Сборка всего проекта"
	@echo "  make clean    # Очистка"
	@echo "  make rebuild  # Полная пересборка"
	@echo "  make test     # Запуск тестов"
	@echo "  make publish  # Сборка для публикации"
