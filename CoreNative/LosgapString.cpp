// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information

#include "LosgapString.h"

#include <codecvt>
#include <sstream>
#include <winerror.h>
#include <cstdarg>

namespace losgap {
	const LosgapString LosgapString::EMPTY { "" };

	const char16_t* copyChar16String(const char16_t* source, size_t strlen) {
		if (source == nullptr) return nullptr;
		char16_t* copy = new char16_t[strlen + 1];
		memcpy_s(copy, (strlen + 1) * sizeof(char16_t), source, (strlen + 1) * sizeof(char16_t));
		return copy;
	}

#pragma region Ctor
	const char16_t* CtorInit(const char16_t* stringToCopy, size_t& outLength) {
		outLength = std::char_traits<char16_t>::length(stringToCopy);
		return copyChar16String(stringToCopy, outLength);
	}
	const char16_t* CtorInit(const char* stringToCopy, size_t& outLength) {
		std::wstring_convert<std::codecvt_utf8_utf16<char16_t>, char16_t> converter { };
		std::u16string convertedString = converter.from_bytes(stringToCopy);

		return CtorInit(convertedString.c_str(), outLength);
	}
	const char16_t* CtorInit(const wchar_t* stringToCopy, size_t& outLength) {
		std::wstring wideString { stringToCopy };
		return CtorInit(std::string { wideString.begin(), wideString.end() }.c_str(), outLength);
	}
	const char16_t* CtorInit(const std::string& stringToCopy, size_t& outLength) {
		return CtorInit(stringToCopy.c_str(), outLength);
	}
	const char16_t* CtorInit(const std::wstring& stringToCopy, size_t& outLength) {
		return CtorInit(std::string { stringToCopy.begin(), stringToCopy.end() }.c_str(), outLength);
	}

	void LosgapString::Init(const char16_t* value, const size_t length) {
		this->value = value;
		this->length = length;
	}

	LosgapString::LosgapString(const char16_t* stringToCopy) : value(nullptr), length(-1) {
		if (stringToCopy == nullptr) return;
		size_t outLength;
		const char16_t* result = CtorInit(stringToCopy, outLength);
		Init(result, outLength);
	}
	LosgapString::LosgapString(const char* stringToCopy) : value(nullptr), length(-1) {
		if (stringToCopy == nullptr) return;
		size_t outLength;
		const char16_t* result = CtorInit(stringToCopy, outLength);
		Init(result, outLength);
	}
	LosgapString::LosgapString(const wchar_t* stringToCopy) : value(nullptr), length(-1) {
		if (stringToCopy == nullptr) return;
		size_t outLength;
		const char16_t* result = CtorInit(stringToCopy, outLength);
		Init(result, outLength);
	}
	LosgapString::LosgapString(const std::string& stringToCopy) : value(nullptr), length(-1) {
		size_t outLength;
		const char16_t* result = CtorInit(stringToCopy, outLength);
		Init(result, outLength);
	}
	LosgapString::LosgapString(const std::wstring& stringToCopy) : value(nullptr), length(-1) {
		size_t outLength;
		const char16_t* result = CtorInit(stringToCopy, outLength);
		Init(result, outLength);
	}
	LosgapString::LosgapString(const LosgapString& c) : value(AsNewChar16String(c).release()), length(c.length) { }
	LosgapString::~LosgapString() {
		SAFE_DELETE_ARR(value);
	}
#pragma endregion

	size_t LosgapString::GetLength() const {
		return length;
	}

#pragma region AsNewXXX
	std::unique_ptr<const char16_t> LosgapString::AsNewChar16String(const LosgapString& string) {
		size_t strlen = std::char_traits<char16_t>::length(string.value);
		return std::unique_ptr<const char16_t> { copyChar16String(string.value, strlen) };
	}
	std::unique_ptr<const char> LosgapString::AsNewCString(const LosgapString& string) {
		std::wstring_convert<std::codecvt_utf8_utf16<char16_t>, char16_t> converter;
		std::string stdString = converter.to_bytes(string.value);

		size_t strlen = stdString.length();
		char* strCopy = new char[strlen + 1];
		strcpy_s(strCopy, strlen + 1, stdString.c_str());

		return std::unique_ptr<const char> { strCopy };
	}
	std::unique_ptr<const wchar_t> LosgapString::AsNewCWString(const LosgapString& string) {
		std::wstring_convert<std::codecvt_utf8_utf16<char16_t>, char16_t> converter;
		std::string stdString = converter.to_bytes(string.value);
		std::wstring stdWstring { stdString.begin(), stdString.end() };

		size_t strlen = stdWstring.length();
		wchar_t* strCopy = new wchar_t[strlen + 1];
		wcsncpy_s(strCopy, strlen + 1, stdWstring.c_str(), strlen + 1);

		return std::unique_ptr<const wchar_t> { strCopy };
	}
	std::string LosgapString::AsNewString(const LosgapString& string) {
		std::wstring_convert<std::codecvt_utf8_utf16<char16_t>, char16_t> converter;
		return converter.to_bytes(string.value);
	}
	std::wstring LosgapString::AsNewWString(const LosgapString& string) {
		std::wstring_convert<std::codecvt_utf8_utf16<char16_t>, char16_t> converter;
		std::string stdString = converter.to_bytes(string.value);
		return std::wstring { stdString.begin(), stdString.end() };
	}
#pragma endregion

#pragma region Concat
	LosgapString LosgapString::Concat(const LosgapString& stringA) {
		return LosgapString { stringA };
	}
	LosgapString LosgapString::Concat(const LosgapString& stringA, const LosgapString& stringB) {
		return stringA + stringB;
	}
	LosgapString LosgapString::Concat(const LosgapString& stringA, const LosgapString& stringB, const LosgapString& stringC) {
		return stringA + stringB + stringC;
	}
	LosgapString LosgapString::Concat(const LosgapString& stringA, const LosgapString& stringB, const LosgapString& stringC, const LosgapString& stringD) {
		return stringA + stringB + stringC + stringD;
	}
	LosgapString LosgapString::Concat(const LosgapString& stringA, const LosgapString& stringB, const LosgapString& stringC, const LosgapString& stringD, const LosgapString& stringE) {
		return stringA + stringB + stringC + stringD + stringE;
	}
	LosgapString LosgapString::Concat(const LosgapString& stringA, const LosgapString& stringB, const LosgapString& stringC, const LosgapString& stringD, const LosgapString& stringE, const LosgapString& stringF) {
		return stringA + stringB + stringC + stringD + stringE + stringF;
	}
#pragma endregion

#pragma region CopyTo
	void LosgapString::CopyTo(char16_t* dest, size_t destArrayLength) const {
		if (dest == nullptr) return;
		if (length + 1 <= destArrayLength) {
			memcpy_s(dest, destArrayLength * sizeof(char16_t), value, (length + 1) * sizeof(char16_t));
		}
		else {
			memcpy_s(dest, destArrayLength * sizeof(char16_t), value, (destArrayLength - 1) * sizeof(char16_t));
			dest[destArrayLength - 1] = 0;
		}
		size_t charsToCopy = ((length + 1) < destArrayLength ? (length + 1) : destArrayLength);
		
	}
	void LosgapString::CopyTo(char* dest, size_t destArrayLength) const {
		if (dest == nullptr) return;
		std::unique_ptr<const char> asNewCStr = AsNewCString(*this);
		size_t length = strlen(asNewCStr.get());
		if (length + 1 <= destArrayLength) {
			memcpy_s(dest, destArrayLength * sizeof(char), asNewCStr.get(), (length + 1) * sizeof(char));
		}
		else {
			memcpy_s(dest, destArrayLength * sizeof(char), asNewCStr.get(), (destArrayLength - 1) * sizeof(char));
			dest[destArrayLength - 1] = 0;
		}
	}
	void LosgapString::CopyTo(wchar_t* dest, size_t destArrayLength) const {
		if (dest == nullptr) return;
		std::unique_ptr<const wchar_t> asNewCWStr = AsNewCWString(*this);
		size_t length = wcslen(asNewCWStr.get());
		if (length + 1 <= destArrayLength) {
			memcpy_s(dest, destArrayLength * sizeof(wchar_t), asNewCWStr.get(), (length + 1) * sizeof(wchar_t));
		}
		else {
			memcpy_s(dest, destArrayLength * sizeof(wchar_t), asNewCWStr.get(), (destArrayLength - 1) * sizeof(wchar_t));
			dest[destArrayLength - 1] = 0;
		}
	}
#pragma endregion

#pragma region Operators
	LosgapString& LosgapString::operator=(const LosgapString& rhs) { 
		SAFE_DELETE_ARR(value);
		value = AsNewChar16String(rhs).release();
		length = rhs.length;

		return *this;
	}
	LosgapString LosgapString::operator+(const LosgapString& rhs) const {
		size_t newStrlen = length + rhs.length;
		char16_t* newString = new char16_t[newStrlen + 1];
		memcpy_s(newString, (newStrlen + 1) * sizeof(char16_t), value, length * sizeof(char16_t));
		memcpy_s(newString + length, (newStrlen + 1 - length) * sizeof(char16_t), rhs.value, (rhs.length + 1) * sizeof(char16_t));
		newString[newStrlen] = '\0';
		return { newString };
	}

	std::ostream& operator<<(std::ostream& lhs, const LosgapString& rhs) {
		lhs << rhs.AsNewCString(rhs).get();
		return lhs;
	}

	// TODO these are inefficient
	LosgapString operator+(const char16_t* lhs, const LosgapString& rhs) {
		return LosgapString { LosgapString { lhs } +rhs };
	}
	LosgapString operator+(const LosgapString& lhs, const char16_t* rhs) {
		return LosgapString { lhs + LosgapString { rhs } };
	}
	LosgapString operator+(const char* lhs, const LosgapString& rhs) {
		return LosgapString { LosgapString { lhs } +rhs };
	}
	LosgapString operator+(const LosgapString& lhs, const char* rhs) {
		return LosgapString { lhs + LosgapString { rhs } };
	}
	LosgapString operator+(const wchar_t* lhs, const LosgapString& rhs) {
		return LosgapString { LosgapString { lhs } +rhs };
	}
	LosgapString operator+(const LosgapString& lhs, const wchar_t* rhs) {
		return LosgapString { lhs + LosgapString { rhs } };
	}
	LosgapString operator+(const std::string& lhs, const LosgapString& rhs) {
		return LosgapString { LosgapString { lhs } +rhs };
	}
	LosgapString operator+(const LosgapString& lhs, const std::string& rhs) {
		return LosgapString { lhs + LosgapString { rhs } };
	}
	LosgapString operator+(const std::wstring& lhs, const LosgapString& rhs) {
		return LosgapString { LosgapString { lhs } +rhs };
	}
	LosgapString operator+(const LosgapString& lhs, const std::wstring& rhs) {
		return LosgapString { lhs + LosgapString { rhs } };
	}
#pragma endregion
}

losgap::LosgapString __declspec(dllexport) losgapMacro::GetHResultDescription(uint32_t result) {
	switch (result) {
		case E_INVALIDARG:
			return "The application supplied an invalid argument to an internal call. Please check that all inputs make sense and are valid at the point of the error.";
		case DXGI_ERROR_DEVICE_HUNG:
			return "Device was not started due to bad input from the application.";
		case DXGI_ERROR_DEVICE_REMOVED:
			return "Device was removed or updated.";
		case DXGI_ERROR_DEVICE_RESET:
			return "Device was reset due to bad input from the application.";
		case DXGI_ERROR_DRIVER_INTERNAL_ERROR:
			return "Device driver encountered an internal error. The device driver may be corrupt or require updating.";
		case DXGI_ERROR_GRAPHICS_VIDPN_SOURCE_IN_USE:
			return "Could not acquire sole graphics functionality. Another graphics application or game may already be using the card.";
		case DXGI_ERROR_INVALID_CALL:
			return "The application made a bad request to the video adapter.";
		case DXGI_ERROR_MORE_DATA:
			return "The application failed to provide enough memory for a call.";
		case DXGI_ERROR_NOT_CURRENTLY_AVAILABLE:
			return "The operation is not currently available.";
		case DXGI_ERROR_NOT_FOUND:
			return "The provided UUID or hardware index was not found.";
		case DXGI_ERROR_WAS_STILL_DRAWING:
			return "Request made to the device when the previous draw attempt had not finished.";
		case DXGI_ERROR_UNSUPPORTED:
			return "The installed hardware does not support the necessary operation.";
		case DXGI_ERROR_WAIT_TIMEOUT:
			return "The adapter timed out when waiting for a frame update.";
		case DXGI_ERROR_RESTRICT_TO_OUTPUT_STALE:
			return "The output chain target (monitor) was altered or disconnected.";
		case DXGI_ERROR_CANNOT_PROTECT_CONTENT:
			return "The adapter or driver is out-of-date (can not protect content).";
		case DXGI_ERROR_ACCESS_DENIED:
			return "The application could not get sufficient access (are you running as administrator?).";
		case DXGI_ERROR_NAME_ALREADY_EXISTS:
			return "The application could not instantiate or name a resource (is another copy already running?).";
		default:
			std::stringstream oss;
			oss << "Unknown error encountered: 0x" << std::hex << result;
			return oss.str();
	}
}