// All code copyright (c) 2014 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information

#include "LosgapCore.h"

EXPORT(ReturnSuccess) {
	EXPORT_END;
}
EXPORT(ReturnFailure, const char16_t* const customFailureMessage) {
	EXPORT_FAIL(customFailureMessage);
	EXPORT_END;
}