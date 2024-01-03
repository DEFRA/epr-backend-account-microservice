run: ## Run the application
	./buildscripts/run.sh

all-tests: ## Run unit tests
	./buildscripts/all-tests.sh

unit-tests: ## Run unit tests
	./buildscripts/unit-tests.sh

build-laps: ## Build laps dev images
	cd src && docker-compose -f docker-compose.yml -f docker-compose.laps.yml build --no-cache

run-laps: ## Run laps dev environment with seeded data
	./buildscripts/run-laps.sh

-include buildscripts/util.make