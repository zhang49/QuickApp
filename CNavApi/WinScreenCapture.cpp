#include "pch.h"
#include "WinScreenCapture.h"
#include <atlstr.h>



WinScreenCapture::WinScreenCapture()
{
	mBeginTick = 0;
	mCaptureCount = 0;
}

int WinScreenCapture::CreateD3dDevice(IDXGIAdapter* adapter, ID3D11Device** device)
{
	int error = 0;
	do {
		PFN_D3D11_CREATE_DEVICE create_device =
			(PFN_D3D11_CREATE_DEVICE)GetProcAddress(mD3d11, "D3D11CreateDevice");
		if (!create_device) {
			error = AE_D3D_GET_PROC_FAILED;
			break;
		}

		HRESULT hr = S_OK;

		// Driver types supported
		// If you set the pAdapter parameter to a non - NULL value, 
		// you must also set the DriverType parameter to the D3D_DRIVER_TYPE_UNKNOWN value.
		D3D_DRIVER_TYPE driver_types[] =
		{
			D3D_DRIVER_TYPE_UNKNOWN,
			D3D_DRIVER_TYPE_HARDWARE,
			D3D_DRIVER_TYPE_WARP,
			D3D_DRIVER_TYPE_REFERENCE,
		};
		UINT n_driver_types = ARRAYSIZE(driver_types);

		// Feature levels supported
		D3D_FEATURE_LEVEL feature_levels[] =
		{
			D3D_FEATURE_LEVEL_11_0,
			D3D_FEATURE_LEVEL_10_1,
			D3D_FEATURE_LEVEL_10_0,
			D3D_FEATURE_LEVEL_9_1
		};
		UINT n_feature_levels = ARRAYSIZE(feature_levels);

		D3D_FEATURE_LEVEL feature_level;

		// Create device
		for (UINT driver_index = 0; driver_index < n_driver_types; ++driver_index)
		{
			hr = create_device(adapter, driver_types[driver_index], nullptr, 0, feature_levels, n_feature_levels,
				D3D11_SDK_VERSION, device, &feature_level, &mD3dCtx);
			if (SUCCEEDED(hr)) break;
		}

		if (FAILED(hr))
		{
			error = AE_D3D_CREATE_DEVICE_FAILED;
			break;
		}

	} while (0);

	return error;
}

//获取屏幕适配器
int WinScreenCapture::GetDstAdapter(IDXGIAdapter** adapter)
{
	int error = AE_NO;
	do {
		auto adapters = d3d_helper::get_adapters(&error, true);
		if (error != AE_NO || adapters.size() == 0)
			break;

		for (std::list<IDXGIAdapter*>::iterator itr = adapters.begin(); itr != adapters.end(); itr++) {
			IDXGIOutput* adapter_output = nullptr;
			DXGI_ADAPTER_DESC adapter_desc = { 0 };
			DXGI_OUTPUT_DESC adapter_output_desc = { 0 };
			(*itr)->GetDesc(&adapter_desc);
			char* tmpStr = CW2A(adapter_desc.Description);
			TraceMsg("adaptor:%s", tmpStr);

			unsigned int n = 0;
			RECT output_rect;
			while ((*itr)->EnumOutputs(n, &adapter_output) != DXGI_ERROR_NOT_FOUND)
			{
				HRESULT hr = adapter_output->GetDesc(&adapter_output_desc);
				if (FAILED(hr)) continue;

				output_rect = adapter_output_desc.DesktopCoordinates;
				tmpStr = CW2A(adapter_output_desc.DeviceName);
				TraceMsg("  output:%s left:%d top:%d right:%d bottom:%d", tmpStr, output_rect.left, output_rect.top, output_rect.right, output_rect.bottom);

				if (output_rect.left <= _rect.left &&
					output_rect.top <= _rect.top &&
					output_rect.right >= _rect.right &&
					output_rect.bottom >= _rect.bottom) {
					error = AE_NO;
					break;
				}

				++n;
			}

			if (error != AE_DXGI_FOUND_ADAPTER_FAILED) {
				*adapter = *itr;
				break;
			}
		}

	} while (0);

	return error;
}

int WinScreenCapture::InitD3d11()
{
	int error = AE_NO;

	do {
		IDXGIAdapter* adapter = nullptr;
		error = GetDstAdapter(&adapter);
		if (error != AE_NO)
			break;

		//error = CreateD3dDevice(adapter, &mD3dDevice);
		error = CreateD3dDevice(nullptr, &mD3dDevice);
		if (error != AE_NO)
			break;
		//No need for grab full screen,but in move & dirty rects copy

	} while (0);

	return error;
}

int WinScreenCapture::InitDuplication()
{
	int error = AE_NO;
	do {
		// Get DXGI device
		IDXGIDevice* dxgi_device = nullptr;
		HRESULT hr = mD3dDevice->QueryInterface(__uuidof(IDXGIDevice), reinterpret_cast<void**>(&dxgi_device));
		if (FAILED(hr))
		{
			error = AE_D3D_QUERYINTERFACE_FAILED;
			break;
		}

		// Get DXGI adapter
		IDXGIAdapter* dxgi_adapter = nullptr;
		hr = dxgi_device->GetParent(__uuidof(IDXGIAdapter), reinterpret_cast<void**>(&dxgi_adapter));
		dxgi_device->Release();
		dxgi_device = nullptr;
		if (FAILED(hr))
		{
			error = AE_DUP_GET_PARENT_FAILED;
			break;
		}

		// Get output
		IDXGIOutput* dxgi_output = nullptr;
		hr = dxgi_adapter->EnumOutputs(mOutputIndex, &dxgi_output);
		dxgi_adapter->Release();
		dxgi_adapter = nullptr;
		if (FAILED(hr))
		{
			error = AE_DUP_ENUM_OUTPUT_FAILED;
			break;
		}

		dxgi_output->GetDesc(&mOutputDesc);

		// QI for Output 1
		IDXGIOutput1* dxgi_output1 = nullptr;
		hr = dxgi_output->QueryInterface(__uuidof(dxgi_output1), reinterpret_cast<void**>(&dxgi_output1));
		dxgi_output->Release();
		dxgi_output = nullptr;
		if (FAILED(hr))
		{
			error = AE_DUP_QI_FAILED;
			break;
		}

		// Create desktop duplication
		hr = dxgi_output1->DuplicateOutput(mD3dDevice, &mDuplication);
		dxgi_output1->Release();
		dxgi_output1 = nullptr;
		if (FAILED(hr))
		{
			error = AE_DUP_DUPLICATE_FAILED;
			if (hr == DXGI_ERROR_NOT_CURRENTLY_AVAILABLE)
			{
				error = AE_DUP_DUPLICATE_MAX_FAILED;
			}

			TraceMsg("duplicate output failed,%lld", hr);
			break;
		}
	} while (0);

	return error;
}

int WinScreenCapture::Init(RECT rect, OutputBits outputBits, bool verticalFlip, OnScreenUpdatedFunc onScreenUpdated) {
	mOnScreenUpdatedFunc = onScreenUpdated;
	mOutputBits = outputBits;
	_rect = rect;
	int error = AE_NO;
	do {
		mVerticalFlip = verticalFlip;
		mD3d11 = system_lib::load_system_library("d3d11.dll");
		mDxgi = system_lib::load_system_library("dxgi.dll");

		if (!mD3d11 || !mDxgi) {
			error = AE_D3D_LOAD_FAILED;
			break;
		}

		error = InitD3d11();
		if (error != AE_NO)
			break;

		mWidth = rect.right - rect.left;
		mHeight = rect.bottom - rect.top;
		mBufferSize = (mWidth * 32 + 31) / 32 * mHeight * 4;
		mBuffer = new uint8_t[mBufferSize];

		error = InitDuplication();
		if (error != AE_NO) {
			break;
		}
	} while (0);

	if (error != AE_NO) {
		TraceMsg("%s,last error:%d", err2str(error), GetLastError());
	}
	else {
		StartCapture();
	}
	return error;
}




int WinScreenCapture::GetDesktopImage(DXGI_OUTDUPL_FRAME_INFO* frame_info)
{
	DWORD bTick = GetTickCount64();
	IDXGIResource* dxgi_res = nullptr;

	// Get new frame
	HRESULT hr = mDuplication->AcquireNextFrame(500, frame_info, &dxgi_res);
	int useTime = GetTickCount64() - bTick;
	//TraceMsg("inside useTime:%d", useTime);

	// Timeout will return when desktop has no chane
	if (hr == DXGI_ERROR_WAIT_TIMEOUT) return AE_TIMEOUT;

	if (FAILED(hr))
		return AE_DUP_ACQUIRE_FRAME_FAILED;

	// QI for IDXGIResource
	hr = dxgi_res->QueryInterface(__uuidof(ID3D11Texture2D), reinterpret_cast<void**>(&mImage));
	dxgi_res->Release();
	dxgi_res = nullptr;
	if (FAILED(hr)) return AE_DUP_QI_FRAME_FAILED;

	// Copy old description
	D3D11_TEXTURE2D_DESC frame_desc;
	mImage->GetDesc(&frame_desc);

	// Create a new staging buffer for fill frame image
	ID3D11Texture2D* new_image = NULL;
	frame_desc.Usage = D3D11_USAGE_STAGING;
	frame_desc.CPUAccessFlags = D3D11_CPU_ACCESS_READ | D3D11_CPU_ACCESS_WRITE;
	frame_desc.BindFlags = 0;
	frame_desc.MiscFlags = 0;
	frame_desc.MipLevels = 1;
	frame_desc.ArraySize = 1;
	frame_desc.SampleDesc.Count = 1;
	frame_desc.SampleDesc.Quality = 0;
	hr = mD3dDevice->CreateTexture2D(&frame_desc, NULL, &new_image);
	if (FAILED(hr)) return AE_DUP_CREATE_TEXTURE_FAILED;

	// Copy next staging buffer to new staging buffer
	mD3dCtx->CopyResource(new_image, mImage);

	// Should calc the row pitch ,and compare dst row pitch with frame row pitch
	// Create staging buffer for map bits
	IDXGISurface* dxgi_surface = NULL;
	hr = new_image->QueryInterface(__uuidof(IDXGISurface), (void**)(&dxgi_surface));
	new_image->Release();
	if (FAILED(hr)) return AE_DUP_QI_DXGI_FAILED;

	// Map buff to mapped rect structure
	DXGI_MAPPED_RECT mapped_rect;
	hr = dxgi_surface->Map(&mapped_rect, DXGI_MAP_READ);
	if (FAILED(hr)) return AE_DUP_MAP_FAILED;

	{
		UNIQUE_LOCK(_buffMtx);
		if (mOutputBits == OutputBits::Output32Argb) {
			int dst_offset_x = _rect.left - mOutputDesc.DesktopCoordinates.left;
			int dst_offset_y = _rect.top - mOutputDesc.DesktopCoordinates.top;
			int dst_rowpitch = min(frame_desc.Width, _rect.right - _rect.left) * 4;
			int dst_colpitch = min(mHeight, mOutputDesc.DesktopCoordinates.bottom - mOutputDesc.DesktopCoordinates.top - dst_offset_y);
			int cpLen = min(mapped_rect.Pitch, dst_rowpitch);
			if (mVerticalFlip) {
				for (int h = 0; h < dst_colpitch; h++) {
					memcpy_s(mBuffer + (dst_colpitch - 1 - h) * dst_rowpitch, dst_rowpitch,
						(BYTE*)mapped_rect.pBits + (h + dst_offset_y) * mapped_rect.Pitch + dst_offset_x * 4, cpLen);
				}
			}
			else {

				for (int h = 0; h < dst_colpitch; h++) {
					memcpy_s(mBuffer + h * dst_rowpitch, dst_rowpitch,
						(BYTE*)mapped_rect.pBits + (h + dst_offset_y) * mapped_rect.Pitch + dst_offset_x * 4, cpLen);
				}
			}
		}
		else if (mOutputBits == OutputBits::Output24Rbg) {
			int dst_offset_x = _rect.left - mOutputDesc.DesktopCoordinates.left;
			int dst_offset_y = _rect.top - mOutputDesc.DesktopCoordinates.top;
			int dst_rowpitch = min(frame_desc.Width, _rect.right - _rect.left) * 4;
			int dst_colpitch = min(mHeight, mOutputDesc.DesktopCoordinates.bottom - mOutputDesc.DesktopCoordinates.top - dst_offset_y);
			int cpLen = min(mapped_rect.Pitch, dst_rowpitch);

			if (mVerticalFlip) {
				//24位图数据
				for (int h = 0; h < dst_colpitch; h++) {
					UINT8* dstPtr = mBuffer + (dst_colpitch - 1 - h) * (min(frame_desc.Width, _rect.right - _rect.left) * 3);
					UINT32* srcPtr = (UINT32*)(mapped_rect.pBits + (h + dst_offset_y) * mapped_rect.Pitch + dst_offset_x * 4);
					for (int w = 0; w < cpLen / 4; w++) {
						*(UINT32*)(dstPtr + w * 3) = *(srcPtr + w);
					}
				}

			}
			else {
				//24位图数据
				for (int h = 0; h < dst_colpitch; h++) {
					UINT8* dstPtr = mBuffer + h * (min(frame_desc.Width, _rect.right - _rect.left) * 3);
					UINT32* srcPtr = (UINT32*)(mapped_rect.pBits + (h + dst_offset_y) * mapped_rect.Pitch + dst_offset_x * 4);
					for (int w = 0; w < cpLen / 4; w++) {
						*(UINT32*)(dstPtr + w * 3) = *(srcPtr + w);
					}
				}




			}
		}
	}
	dxgi_surface->Unmap();
	dxgi_surface->Release();
	dxgi_surface = nullptr;
	return AE_NO;
}


void WinScreenCapture::CleanDuplication()
{
	if (mDuplication) mDuplication->Release();
	if (mImage) mImage->Release();

	mDuplication = nullptr;
	mImage = nullptr;
}


int WinScreenCapture::FreeDuplicatedFrame()
{
	HRESULT hr = mDuplication->ReleaseFrame();
	if (FAILED(hr))
	{
		return AE_DUP_RELEASE_FRAME_FAILED;
	}

	if (mImage)
	{
		mImage->Release();
		mImage = nullptr;
	}

	return AE_DUP_RELEASE_FRAME_FAILED;
}


void WinScreenCapture::StopCapture()
{
	mCaptureRunning = false;
	if (m_CaptureProcessTh.joinable()) {
		m_CaptureProcessTh.join();
	}
}

void WinScreenCapture::StartCapture()
{
	mCaptureRunning = true;
	m_CaptureProcessTh = std::thread(std::bind(&WinScreenCapture::CaptureProcess, this));
}

void WinScreenCapture::CaptureProcess() {
	int error = 0;
	DXGI_OUTDUPL_FRAME_INFO frame_info;
	void(*_on_error)(int) = NULL;
	mBeginTick = GetTickCount64();
	mCaptureCount = 0;
	while (mCaptureRunning)
	{
		//Timeout is no new picture,no need to update
		DWORD tick = GetTickCount64();
		if ((error = GetDesktopImage(&frame_info)) == AE_TIMEOUT) continue;
		if (error != AE_NO) {
			while (mCaptureRunning)
			{
				Sleep(300);
				CleanDuplication();
				if ((error = InitDuplication()) != AE_NO) {
					if (_on_error) _on_error(error);
				}
				else break;
			}
			TraceMsg("get_desktop_image return error:%s", ERRORS_STR[error]);
			continue;
		}
		mCaptureCount++;
		int captureFrequency = 1000.0 / ((GetTickCount64() - mBeginTick) / ((double)mCaptureCount + 1.0));
		//captureFrequency不是fps
		int useTime = GetTickCount64() - tick;
		TraceMsg("use time:%d, capture frequency:%d", useTime, captureFrequency);
		if (mOnScreenUpdatedFunc) {
			mOnScreenUpdatedFunc(mWidth, mHeight, mBuffer, mBufferSize);
		}
		/*if ((error = get_desktop_cursor(&frame_info)) == AE_NO)
			draw_cursor();*/
		FreeDuplicatedFrame();
		Sleep(5);
	}
}

