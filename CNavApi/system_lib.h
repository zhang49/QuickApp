#pragma once
class system_lib
{
	//1.获取系统路径
	static char system_path[MAX_PATH];
public:
	static bool get_system_path();

	//2.加载动态库
	static HMODULE load_system_library(const char* name);

	//释放动态库
	static void free_system_library(HMODULE handle);
};

