#include "pch.h"
#include "BitmapMap.h"

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
	auto size = D2D1::SizeU(width, height);
	auto properties = D2D1::BitmapProperties1(D2D1_BITMAP_OPTIONS_CPU_READ, D2D1::PixelFormat(DXGI_FORMAT_B8G8R8A8_UNORM, D2D1_ALPHA_MODE_IGNORE));

	auto context = GetWrappedResource<ID2D1DeviceContext1>(drawingSession);
	ComPtr<ID2D1Bitmap1> bitmap;
	__abi_ThrowIfFailed(context->CreateBitmap(size, nullptr, 0u, properties, bitmap.GetAddressOf()));
	
	ComPtr<IUnknown> unknown;
	__abi_ThrowIfFailed(bitmap.As(&unknown));
	auto output = GetOrCreate<CanvasBitmap>(drawingSession->Device, unknown.Get());
	return output;
}