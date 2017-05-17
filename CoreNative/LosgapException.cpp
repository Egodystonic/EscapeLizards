// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information

#pragma once

#include "LosgapException.h"

namespace losgap {
	LosgapException::LosgapException(const LosgapString& message) : Message(message) { }
}