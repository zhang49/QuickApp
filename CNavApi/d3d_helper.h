#pragma once
#include <list>
#include <dxgi1_2.h>

class d3d_helper
{
public:
	static std::list<IDXGIAdapter*> get_adapters(int* error, bool free_lib);
};

