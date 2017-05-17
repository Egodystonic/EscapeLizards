// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information

#pragma once

#include <exception>
#include "LosgapString.h"

namespace losgap {
	/* 
	A specialised exception type for errors in the native side of the LOSGAP framework.
	*/
	class __declspec(dllexport) LosgapException {
	public:
		const LosgapString Message;
		LosgapException(const LosgapString& message);
	};
}