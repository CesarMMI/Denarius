import { BottomSheetDirective } from '../directives/bottom-sheet.directive';

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type BottomSheetResult<C> = C extends BottomSheetDirective<any, infer R> ? R : never;
