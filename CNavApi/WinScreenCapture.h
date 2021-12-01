#pragma once
#include "pch.h"
#include <d3d11.h>
#include <dxgi1_2.h>
#include "error_define.h"
#include "system_lib.h"
#include "d3d_helper.h"
#include "Log.h"

#define SHARD_LOCK(x)		std::shared_lock<std::shared_mutex>(x)
#define UNIQUE_LOCK(x)		std::unique_lock<std::shared_mutex>(x)

typedef std::function<void(int w, int h, uint8_t* data, int size)> OnScreenUpdatedFunc;

class WinScreenCapture
{
public:
	enum OutputBits {
		Output24Rbg,
		Output32Argb
	};
	static WinScreenCapture& Instance()
	{
		static WinScreenCapture _instance;
		return _instance;
	}
	//ªÒ»°∆¡ƒª  ≈‰∆˜
	int GetDstAdapter(IDXGIAdapter** adapter);
	int Init(RECT rect, OutputBits outputBits, bool verticalFlip, OnScreenUpdatedFunc onScreenUpdated);

private:
	WinScreenCapture();
	int InitD3d11();
	int InitDuplication();
	int CreateD3dDevice(IDXGIAdapter* adapter, ID3D11Device** device);
	int GetDesktopImage(DXGI_OUTDUPL_FRAME_INFO* frame_info);
	void CleanDuplication();
	int FreeDuplicatedFrame();
	void CaptureProcess();
	void StartCapture();
	void StopCapture();


private:
	int mOutputIndex = 0;
	OnScreenUpdatedFunc mOnScreenUpdatedFunc;
	ID3D11DeviceContext* mD3dCtx;
	RECT _rect;
	int mWidth;
	int mHeight;
	bool mVerticalFlip;
	OutputBits mOutputBits;

	ID3D11Device* mD3dDevice;
	DXGI_OUTPUT_DESC mOutputDesc;
	IDXGIOutputDuplication* mDuplication;

	ID3D11Texture2D* mImage;
	size_t mBufferSize;

	std::shared_mutex mBuffMtx;
	uint8_t* mBuffer;

	HMODULE mD3d11;
	HMODULE mDxgi;

	std::atomic<bool> mCaptureRunning = false;
	std::atomic<DWORD> mBeginTick;
	std::atomic<int> mCaptureCount = 0;

	std::thread m_CaptureProcessTh;
};

