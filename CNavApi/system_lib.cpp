#include "pch.h"
#include "Log.h"
#include "system_lib.h"

//1.��ȡϵͳ·��
char system_lib::system_path[260] = { 0 };
bool system_lib::get_system_path() {

	if (strlen(system_path) == 0) {
		UINT ret = GetSystemDirectoryA(system_path, MAX_PATH);
		if (!ret) {
			TraceMsg("failed to get system directory :%lu", GetLastError());
			return false;
		}
	}

	return true;
}

//2.���ض�̬��
HMODULE system_lib::load_system_library(const char* name)
{
	if (get_system_path() == false) return NULL;

	char base_path[MAX_PATH] = { 0 };
	strcpy_s(base_path, system_path);
	strcat_s(base_path, "\\");
	strcat_s(base_path, name);

	HMODULE module = GetModuleHandleA(base_path);
	if (module)
		return module;

	module = LoadLibraryA(base_path);
	if (!module) {
		TraceMsg("failed load system library :%lu", GetLastError());
	}

	return module;
}

//�ͷŶ�̬��
void system_lib::free_system_library(HMODULE handle)
{
	FreeModule(handle);
}