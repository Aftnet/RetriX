#include "pch.h"
#include "D3DSurfaceMap.h"

using namespace Retrix::UWP::Native;
using namespace Platform;

D3DSurfaceMap::D3DSurfaceMap(CanvasDevice^ device, IDirect3DSurface^ surface)
{
	ComPtr<ID3D11Device> d3dDevice;
	__abi_ThrowIfFailed(GetDXGIInterface(device, d3dDevice.GetAddressOf()));

	d3dDevice->GetImmediateContext(d3dContext.GetAddressOf());
	__abi_ThrowIfFailed(GetDXGIInterface(surface, d3dTexture.GetAddressOf()));

	D3D11_MAPPED_SUBRESOURCE mappedResource;
	__abi_ThrowIfFailed(d3dContext->Map(d3dTexture.Get(), 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource));

	pitchBytes = mappedResource.RowPitch;
	data = (UnsafeIntPtr)mappedResource.pData;
}

D3DSurfaceMap::~D3DSurfaceMap()
{
	d3dContext->Unmap(d3dTexture.Get(), 0);
}

IDirect3DSurface^ D3DSurfaceMap::CreateMappableD3DSurface(CanvasDevice^ device, unsigned int width, unsigned int height)
{
	ComPtr<ID3D11Device> d3dDevice;
	__abi_ThrowIfFailed(GetDXGIInterface(device, d3dDevice.GetAddressOf()));

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
	return winRTSurface;
}