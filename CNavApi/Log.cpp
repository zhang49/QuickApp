#include "pch.h"
#include "Log.h"


void Log::Trace(const char* format, ...)
{
	static const int MAX_SIZE_LOG = 4096;
	char buf[MAX_SIZE_LOG] = { 0 };
	va_list arglist;
	va_start(arglist, format);
	vsnprintf_s(buf, MAX_SIZE_LOG, format, arglist);
	va_end(arglist);
	//printf("%s\n", buf);
	OutputDebugStringA(buf);
	OutputDebugStringA("\n");
}