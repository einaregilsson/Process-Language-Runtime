

ccs: plr
	gmcs  -t:exe -reference:bin/PLR.dll -reference:bin/nunit.framework.dll.mono -recurse:CCS/*.cs -out:bin/CCS.exe
	@echo #!/bin/sh > ccs
	@echo mono ./bin/CCS.exe \$$1 \$$2 \$$3 \$$4 \$$5 >> ccs
	@chmod +x ccs 

plr: clean
	gmcs -t:library -recurse:PLR/*.cs -out:bin/PLR.dll

clean:
	@rm -f bin/PLR.dll
	@rm -f bin/CCS.exe
	@rm -f ccs
