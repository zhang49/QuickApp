#pragma once
class system_lib
{
	//1.��ȡϵͳ·��
	static char system_path[MAX_PATH];
public:
	static bool get_system_path();

	//2.���ض�̬��
	static HMODULE load_system_library(const char* name);

	//�ͷŶ�̬��
	static void free_system_library(HMODULE handle);
};

