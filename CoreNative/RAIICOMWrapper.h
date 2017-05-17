// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information

#pragma once
#include "../CoreNative/LosgapCore.h"

namespace losgap {
	/*
	A wrapper that makes sure a COM object is released at the end of the scope
	*/
	template <class T>
	class RAIICOMWrapper {
	private:
		T comObjectPtr;
	public:
		DEFINE_DEFAULT_CONSTRUCTOR(RAIICOMWrapper);
		RAIICOMWrapper(T comObjectPtr) : comObjectPtr(comObjectPtr) { }
		~RAIICOMWrapper() { RELEASE_COM(comObjectPtr); }

		T Get() { return comObjectPtr; }
		T* GetPtr() { return &comObjectPtr; }
		void Set(T comObjectPtr) { this->comObjectPtr = comObjectPtr; }
	};
}