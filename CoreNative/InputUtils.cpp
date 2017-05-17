// All code copyright (c) 2015 Ophidian Games || http://www.ophidian-games.com
// See http://www.losgap.com/ for licensing information
// Created on 15 03 2015 at 18:26 by Raphael Rabl

#include "InputUtils.h"

namespace losgap {

	/*
	Get the array of all virtual keys from outside and save it
	*/
	void InputUtils::PassVirtualKeys(int32_t* virtualKeys) {
		if (virtualKeys == nullptr) throw LosgapException{ "Array 'virtualKeys' must not be null." };
		VirtualKeys = virtualKeys;
	}

	/*
	Fill the given pointer with an array of all the current key states
	*/
	void InputUtils::FillVirtualKeyStateArray(VirtualKeyState* keyStateArr, const int32_t arrLen) {
		if (keyStateArr == nullptr) throw LosgapException{ "Array 'virtualKeys' must not be null." };
		if (arrLen <= 0) throw LosgapException{ "Array 'virtualKeys' length must not be negative or 0." };

		for (int i = 0; i < arrLen; ++i)
		{
			keyStateArr[i] = (GetAsyncKeyState(VirtualKeys[i]) & KEY_DOWN_BIT) != 0 ? VirtualKeyState::KeyDown : VirtualKeyState::KeyUp;
		}
	}

	/*
	Fill the given pointers with the current cursor X and Y position
	*/
	void InputUtils::FetchCursorPosition(float_t& outX, float_t& outY) {
		POINT cursorPos = POINT();
		LPPOINT pCursorPos = &cursorPos;
		GetCursorPos(pCursorPos);
		outX = static_cast<float_t>(pCursorPos->x);
		outY = static_cast<float_t>(pCursorPos->y);
	}

	/*
	Set the cursor to position (X, Y)
	*/
	void InputUtils::SetCursorPosition(float_t x, float_t y) {
		SetCursorPos(static_cast<int>(x), static_cast<int>(y));
	}

	EXPORT(InputUtils_PassVirtualKeys, int32_t* virtualKeys) {
		InputUtils::PassVirtualKeys(virtualKeys);
		EXPORT_END;
	}

	EXPORT(InputUtils_FillVirtualKeyStateArray, VirtualKeyState* keyStateArr, int32_t arrLen) {
		InputUtils::FillVirtualKeyStateArray(keyStateArr, arrLen);
		EXPORT_END;
	}

	EXPORT(InputUtils_FetchCursorPosition, float_t& outX, float_t& outY) {
		InputUtils::FetchCursorPosition(outX, outY);
		EXPORT_END;
	}

	EXPORT(InputUtils_SetCursorPosition, float_t x, float_t y) {
		InputUtils::SetCursorPosition(x, y);
		EXPORT_END;
	}
}