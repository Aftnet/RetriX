#include "pch.h"
#include "BitmapMap.h"
#include "D3D11Interop.h"

using namespace Retrix::UWP::Native;
using namespace Platform;

BitmapMap::BitmapMap(CanvasDevice^ device, CanvasBitmap^ bitmap)
{
	d2dBitmap = GetWrappedResource<ID2D1Bitmap1, CanvasBitmap>(device, bitmap);
	__abi_ThrowIfFailed(d2dBitmap->Map(D2D1_MAP_OPTIONS_WRITE | D2D1_MAP_OPTIONS_DISCARD, &map));
}

BitmapMap::~BitmapMap()
{
	__abi_ThrowIfFailed(d2dBitmap->Unmap());
}

CanvasBitmap^ BitmapMap::CreateMappableBitmap(CanvasDrawingSession^ drawingSession, unsigned int width, unsigned int height)
{
	ComPtr<ID3D11Device> d3dDevice(D3D11Interop::GetD3D11DevicePtr(drawingSession));

	D3D11_TEXTURE2D_DESC texDesc = { 0 };
	texDesc.Width = width;
	texDesc.Height = height;
	texDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
	texDesc.MipLevels = 1;
	texDesc.ArraySize = 1;
	texDesc.SampleDesc.Count = 1;
	texDesc.SampleDesc.Quality = 0;
	texDesc.Usage = D3D11_USAGE_DYNAMIC;
	texDesc.BindFlags = D3D11_BIND_SHADER_RESOURCE;
	texDesc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
	texDesc.MiscFlags = 0;

	ComPtr<ID3D11Texture2D> d3dTexture;
	__abi_ThrowIfFailed(d3dDevice->CreateTexture2D(&texDesc, nullptr, d3dTexture.GetAddressOf()));

	ComPtr<IDXGISurface> d3dSurface;
	__abi_ThrowIfFailed(d3dTexture.As(&d3dSurface));

	auto winRTSurface = CreateDirect3DSurface(d3dSurface.Get());
	auto output = CanvasBitmap::CreateFromDirect3D11Surface(drawingSession, winRTSurface);
	return output;
}