import { BottomSheetDirective } from '../directives/bottom-sheet.directive';

export interface BottomSheetOptions<T, R, C extends BottomSheetDirective<T, R>> {
	data?: T;
	callback?: (result: R, component: C) => void;
}
