SET rev=ac22b06

IF NOT EXIST "docs" (

	git clone --filter=tree:0 --sparse https://github.com/MicrosoftDocs/windows-driver-docs-ddi docs
	
	cd docs
	
	git sparse-checkout set wdk-ddi-src/content/dbgeng
	git switch --detach %rev%

) ELSE (

	cd docs
	
	git switch staging
	git pull
	git switch --detach %rev%

)
