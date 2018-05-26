#pragma once

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
			public ref class D3DSurfaceMap sealed
			{
#ifdef _WIN64
				typedef int64 UnsafeIntPtr;
#else
				typedef int32 UnsafeIntPtr;
#endif
			private:
				ComPtr<ID3D11DeviceContext> d3dContext;
				ComPtr<ID3D11Resource> d3dTexture;

				uint32 pitchBytes;
				UnsafeIntPtr data;

			public:
				property uint32 PitchBytes { uint32 get() { return pitchBytes; } }
				property UnsafeIntPtr Data {UnsafeIntPtr get() { return data; } }

				D3DSurfaceMap(CanvasDevice^ device, IDirect3DSurface^ surface);
				virtual ~D3DSurfaceMap();

				static IDirect3DSurface^ CreateMappableD3DSurface(CanvasDevice^ device, unsigned int width, unsigned int height);
			};
		}
	}
}
