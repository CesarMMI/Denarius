import { Directive, inject } from '@angular/core';
import { MatBottomSheetRef, MAT_BOTTOM_SHEET_DATA } from '@angular/material/bottom-sheet';
import { BottomSheetOptions } from '../types/bottom-sheet-options';

@Directive()
export class BottomSheetDirective<T, R> {
	private readonly bottomSheetRef = inject(MatBottomSheetRef);
	private readonly _sheetData = inject<T & Pick<BottomSheetOptions<T, R, BottomSheetDirective<T, R>>, 'callback'>>(
		MAT_BOTTOM_SHEET_DATA,
		{ optional: true },
	);

	get sheetData() {
		return this._sheetData as T;
	}

	close() {
		this.bottomSheetRef.dismiss();
	}

	protected callback(result: R) {
		if (!this._sheetData?.callback) this.close();
		else this._sheetData.callback(result, this);
	}
}
