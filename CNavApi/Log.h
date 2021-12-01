#pragma once

#define TraceMsg(format, ...) Log::Trace(format, ##   __VA_ARGS__)

class Log
{
public:
	static void Trace(const char* format, ...);
};

