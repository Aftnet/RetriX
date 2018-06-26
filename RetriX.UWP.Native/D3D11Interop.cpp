#include "pch.h"
#include "D3D11Interop.h"

using namespace Retrix::UWP::Native;

Retrix::UWP::Native::IntPtr D3D11Interop::GetD3D11Device(CanvasDrawingSession^ drawingSession)
{
	return (IntPtr)GetD3D11DevicePtr(drawingSession);
}

Retrix::UWP::Native::IntPtr D3D11Interop::GetD3D11Context(CanvasDrawingSession^ drawingSession)
{
	ComPtr<ID3D11Device> device(GetD3D11DevicePtr(drawingSession));
	ID3D11DeviceContext* context;
	device->GetImmediateContext(&context);
	return (IntPtr)context;
}

ID3D11Device* D3D11Interop::GetD3D11DevicePtr(CanvasDrawingSession^ drawingSession)
{
	ID3D11Device* ptr;
	__abi_ThrowIfFailed(GetDXGIInterface(drawingSession->Device, &ptr));
	return ptr;
}