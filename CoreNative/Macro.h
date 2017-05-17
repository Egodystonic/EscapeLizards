// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information

#pragma once

#pragma region Alloc/Dealloc

#define ALIGNED_NEW(type, alignment) new(_aligned_malloc(sizeof(type), alignment)) type

#define SAFE_DELETE(ptr)		\
{								\
	delete ptr; ptr = nullptr;	\
}								\

#define SAFE_DELETE_ARR(ptr)		\
{									\
	delete[] ptr; ptr = nullptr;	\
}									\

#define RELEASE_COM(comObj)	\
if (comObj != nullptr) {	\
	comObj->Release();		\
	comObj = nullptr;		\
}

#pragma endregion

#pragma region Constructor/Move/Assign Macros

#define DISALLOW_COPY_ASSIGN_MOVE(TypeName)		\
	TypeName(const TypeName&) = delete;			\
	void operator=(const TypeName&) = delete;	\
	TypeName(TypeName&&) = delete;				\

#define DISALLOW_COPY_ASSIGN(TypeName)			\
	TypeName(const TypeName&) = delete;			\
	void operator=(const TypeName&) = delete;	\

#define DEFINE_DEFAULT_CONSTRUCTOR(TypeName) TypeName::TypeName() = default;
#define DECLARE_DEFAULT_CONSTRUCTOR(TypeName) TypeName();
#define DEFINE_DEFAULT_DESTRUCTOR(TypeName) TypeName::~TypeName() = default;
#define DECLARE_DEFAULT_DESTRUCTOR(TypeName) ~TypeName();

#pragma endregion

#pragma region Export and Interop

#define MAX_INTEROP_FAIL_REASON_STRING_LENGTH 200
#define STRUCT_PACKING_SAFE 1
#define STRUCT_PACKING_OPTIMAL 8
#define INTEROP_STRING const char16_t*
#define INTEROP_BOOL char
#define INTEROP_BOOL_TRUE ((INTEROP_BOOL) 255)
#define INTEROP_BOOL_FALSE ((INTEROP_BOOL) 0)

#define EXPORT(funcName, ...)																			\
	extern "C" __declspec(dllexport) INTEROP_BOOL funcName(char16_t* const failureReason, __VA_ARGS__) {	\
	try																										\

#define EXPORT_END										\
		EXPORT_OK										\
	}													\
	catch (losgap::LosgapException& e) {				\
		EXPORT_FAIL(e.Message);							\
	}													\
	catch (std::exception& e) {							\
		EXPORT_FAIL(e.what());							\
	}													\
	catch (...) {										\
		EXPORT_FAIL("Unknown exception occurred.");		\
	}													\
		
#define INTEROP_BOOL_TO_CBOOL(value) (value != INTEROP_BOOL_FALSE)
#define CBOOL_TO_INTEROP_BOOL(value) (value ? INTEROP_BOOL_TRUE : INTEROP_BOOL_FALSE)

#define EXPORT_FAIL(reason)																	\
{																								\
	losgap::LosgapString failureReasonAsString { reason };										\
	failureReasonAsString.CopyTo(failureReason, MAX_INTEROP_FAIL_REASON_STRING_LENGTH + 1);		\
	return INTEROP_BOOL_FALSE;																	\
}																								\

#define EXPORT_OK return INTEROP_BOOL_TRUE;

#pragma endregion

#pragma region WinAPI
#define DESCRIPTION(hResult) losgapMacro::GetHResultDescription(hResult)

#define CHECK_CALL(call)																	\
{																							\
	HRESULT checkCallResult = call;															\
	if (FAILED(checkCallResult)) {															\
		throw losgap::LosgapException { DESCRIPTION(checkCallResult) };						\
	}																						\
}																							\

#ifdef DEBUG
#define Debug(text) OutputDebugString(text)
#else
#define Debug(text)
#endif

#pragma endregion