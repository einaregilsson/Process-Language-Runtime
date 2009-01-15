@echo off
coco ccs.atg -namespace CCS.Parsing -o ..
if ERRORLEVEL 0 (
    if exist ..\Parser.cs.old del ..\Parser.cs.old
    if exist ..\Scanner.cs.old del ..\Scanner.cs.old
)