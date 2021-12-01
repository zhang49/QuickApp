#include "WinScreenCapture.h"

#define CAPI(type) extern "C" __declspec(dllexport) type __stdcall

CAPI(int) Init(int l, int t, int w, int h, int outputBits, int verticalFlip, void(*OnUpdated)(int w, int h, uint8_t* data, int size))
{
	WinScreenCapture::OutputBits ob;
	if (outputBits == 24) {
		ob = WinScreenCapture::OutputBits::Output24Rbg;
	}
	else if (outputBits == 32) {
		ob = WinScreenCapture::OutputBits::Output32Argb;
	}
	else {
		//param error.
		return 0xff;
	}
	return WinScreenCapture::Instance().Init({ l,t,w,h }, ob, verticalFlip != 0, OnUpdated);
}

