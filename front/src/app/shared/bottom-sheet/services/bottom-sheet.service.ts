import { ComponentType } from '@angular/cdk/overlay';
import { inject, Injectable } from '@angular/core';
import { MatBottomSheet } from '@angular/material/bottom-sheet';
import { BottomSheetDirective } from '../directives/bottom-sheet.directive';
import { BottomSheetData } from '../types/bottom-sheet-data';
import { BottomSheetOptions } from '../types/bottom-sheet-options';
import { BottomSheetResult } from '../types/bottom-sheet-result';

@Injectable({ providedIn: 'root' })
export class BottomSheetService {
	private readonly matBottomSheet = inject(MatBottomSheet);

	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	open<C extends BottomSheetDirective<any, any>>(
		component: ComponentType<C>,
		options?: BottomSheetOptions<BottomSheetData<C>, BottomSheetResult<C>, C>,
	) {
		this.matBottomSheet.open(component, {
			data: { ...options?.data, callback: options?.callback },
			panelClass: 'app-bottom-sheet',
		});
	}
}
