#pragma once

#include "IntPtr.h"

using namespace Microsoft::Graphics::Canvas;
using namespace Microsoft::WRL;
using namespace Platform;
using namespace Windows::Graphics::DirectX::Direct3D11;

namespace Retrix
{
	namespace UWP
	{
		namespace Native
		{
			public ref class BitmapMap sealed
			{
			private:
				ComPtr<ID2D1Bitmap1> d2dBitmap;
				D2D1_MAPPED_RECT map;

			public:
				property uint32 PitchBytes { uint32 get() { return map.pitch; } }
				property IntPtr Data { IntPtr get() { return (IntPtr)map.bits; } }

				BitmapMap(CanvasDevice^ device, CanvasBitmap^ bitmap);
				virtual ~BitmapMap();

				static CanvasBitmap^ CreateMappableBitmap(CanvasDrawingSession^ drawingSession, unsigned int width, unsigned int height);
			};
		}
	}
}
