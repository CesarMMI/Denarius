import { BottomSheetDirective } from '../directives/bottom-sheet.directive';

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type BottomSheetData<C> = C extends BottomSheetDirective<infer T, any> ? T : never;
