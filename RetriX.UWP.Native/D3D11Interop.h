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
			static public ref class D3D11Interop sealed
			{
			public:
				static IntPtr GetD3D11Device(CanvasDrawingSession^ drawingSession);
				static IntPtr GetD3D11Context(CanvasDrawingSession^ drawingSession);
			internal:
				static ID3D11Device* GetD3D11DevicePtr(CanvasDrawingSession^ drawingSession);
			};
		}
	}
}

