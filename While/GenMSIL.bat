@echo off
%WINDIR%\Microsoft.NET\Framework\v2.0.50727\csc.exe /debug /out:bin\debug\CSharpTest.exe CSharpTest.cs
ildasm /out=CSharpTest.MSIL /item=CSharpTest::Main bin\debug\CSharpTest.exe
del bin\debug\CSharpTest.exe
del CSharpTest.res
del *.old