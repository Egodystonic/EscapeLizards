// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information

#pragma once

#include "Macro.h"

#include <cstdint>
#include <memory>
#include <string>

namespace losgap {
	/* 
	A low performance but high usability string wrapper, encompassing the five million other string types in C++.
	*/
	class __declspec(dllexport) LosgapString {
	private:
		const char16_t* value;
		size_t length;

		void Init(const char16_t* value, const size_t length);

	public:
		static const LosgapString EMPTY;

		LosgapString(const char16_t* stringToCopy);
		LosgapString(const char* stringToCopy);
		LosgapString(const wchar_t* stringToCopy);
		LosgapString(const std::string& stringToCopy);
		LosgapString(const std::wstring& stringToCopy);
		LosgapString(const LosgapString& c);
		DECLARE_DEFAULT_DESTRUCTOR(LosgapString);

		size_t GetLength() const;

		static std::unique_ptr<const char16_t> AsNewChar16String(const LosgapString& string);
		static std::unique_ptr<const char> AsNewCString(const LosgapString& string);
		static std::unique_ptr<const wchar_t> AsNewCWString(const LosgapString& string);
		static std::string AsNewString(const LosgapString& string);
		static std::wstring AsNewWString(const LosgapString& string);

		static LosgapString Concat(const LosgapString& stringA);
		static LosgapString Concat(const LosgapString& stringA, const LosgapString& stringB);
		static LosgapString Concat(const LosgapString& stringA, const LosgapString& stringB, const LosgapString& stringC);
		static LosgapString Concat(const LosgapString& stringA, const LosgapString& stringB, const LosgapString& stringC, const LosgapString& stringD);
		static LosgapString Concat(const LosgapString& stringA, const LosgapString& stringB, const LosgapString& stringC, const LosgapString& stringD, const LosgapString& stringE);
		static LosgapString Concat(const LosgapString& stringA, const LosgapString& stringB, const LosgapString& stringC, const LosgapString& stringD, const LosgapString& stringE, const LosgapString& stringF);

		void CopyTo(char16_t* dest, size_t destArrayLength) const;
		void CopyTo(char* dest, size_t destArrayLength) const;
		void CopyTo(wchar_t* dest, size_t destArrayLength) const;
		
		LosgapString& operator=(const LosgapString& rhs);
		LosgapString operator+(const LosgapString& rhs) const;
		friend __declspec(dllexport) LosgapString operator+(const LosgapString& lhs, const char16_t* rhs);
		friend __declspec(dllexport) LosgapString operator+(const char16_t* lhs, const LosgapString& rhs);
		friend __declspec(dllexport) LosgapString operator+(const LosgapString& lhs, const char* rhs);
		friend __declspec(dllexport) LosgapString operator+(const char* lhs, const LosgapString& rhs);
		friend __declspec(dllexport) LosgapString operator+(const LosgapString& lhs, const wchar_t* rhs);
		friend __declspec(dllexport) LosgapString operator+(const wchar_t* lhs, const LosgapString& rhs);
		friend __declspec(dllexport) LosgapString operator+(const LosgapString& lhs, const std::string& rhs);
		friend __declspec(dllexport) LosgapString operator+(const std::string& lhs, const LosgapString& rhs);
		friend __declspec(dllexport) LosgapString operator+(const LosgapString& lhs, const std::wstring& rhs);
		friend __declspec(dllexport) LosgapString operator+(const std::wstring& lhs, const LosgapString& rhs);
		friend __declspec(dllexport) std::ostream& operator<<(std::ostream& lhs, const LosgapString& rhs);
	};
}

namespace losgapMacro {
	losgap::LosgapString __declspec(dllexport) GetHResultDescription(uint32_t result);
}