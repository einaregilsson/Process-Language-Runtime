#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
#include <string>
#include <windows.h>
#include <stdio.h>
#include <iostream>
#include <vector>

using namespace std;

int __stdcall WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nShowCmd)
{
    MSG msg; 
    WCHAR buf[256]; 
    buf[0] = 0;
    if (!RegisterHotKey(0, 1, MOD_WIN, 0x43)) {
        MessageBox(NULL, L"Failed to register hotkey!", L"Filename AutoComplete", MB_OK);
        return 1;
    }
    while (GetMessage(&msg, NULL, 0, 0)) {
        if (msg.message == WM_HOTKEY) {
            HWND hWnd = GetForegroundWindow();
		    UINT remoteThreadId = GetWindowThreadProcessId(hWnd, NULL);
		    UINT currentThreadId = GetCurrentThreadId();
    		BOOL res = AttachThreadInput(remoteThreadId, currentThreadId, TRUE);
            HWND editbox = GetFocus();
            AttachThreadInput(remoteThreadId, currentThreadId, FALSE);
            LRESULT count = SendMessage(editbox, WM_GETTEXT, 255, (LPARAM)buf);

			wstring path(buf);
			wstring file(L"");
			size_t pos = path.find_last_of(L"\\");
			if (pos == -1) {
				continue;
			}
			wstring base = path.substr(0, pos+1);
			if ((GetFileAttributes(base.c_str()) & FILE_ATTRIBUTE_DIRECTORY) != FILE_ATTRIBUTE_DIRECTORY) {
				continue;
			}

			if (GetFileAttributes(path.c_str()) != 0xFFFFFFFF) { //The full path is a valid file, lets use the next file we find
				file = path.substr(pos+1);
				path = path.substr(0, pos+1);
			}

			wstring query = path + L"*";
			WIN32_FIND_DATA ffd;
			HANDLE hFind = INVALID_HANDLE_VALUE;
			hFind = FindFirstFile(query.c_str(), &ffd);
			if (INVALID_HANDLE_VALUE == hFind) 
			   continue;
			
			vector<wstring> files;
			do {
				if (ffd.cFileName[0] != L'.') {
					files.push_back(wstring(ffd.cFileName));
				}
			} while (FindNextFile(hFind, &ffd) != 0);
			FindClose(hFind);
			wstring fullpath;
			
			if (files.size() == 1) {
				fullpath = base + files[0];
				SendMessage(editbox, WM_SETTEXT, 0, (LPARAM)fullpath.c_str());
			} else {
				for (int i = 0; i < files.size(); i++) {
					//MessageBox(NULL, files[i].c_str(), L"Filename AutoComplete", MB_OK);						
					//MessageBox(NULL, file.c_str(), L"Filename AutoComplete", MB_OK);						
					if (files[i] == file) {
						//MessageBox(NULL, L"WAS EQ", L"Filename AutoComplete", MB_OK);						
						int index = i+1;
						if (index == files.size()) index--;
						fullpath = base + files[index];
						//MessageBox(NULL, fullpath.c_str(), L"Filename AutoComplete", MB_OK);						
						SendMessage(editbox, WM_SETTEXT, 0, (LPARAM)fullpath.c_str());
					}
				}
			}
			//WCHAR b[300];
			//swprintf(b, L"count is %d", files.size());
			//MessageBox(NULL, b, L"Filename AutoComplete", MB_OK);

		}
    }
    return 0;
}

