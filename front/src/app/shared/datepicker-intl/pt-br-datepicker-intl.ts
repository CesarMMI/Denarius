import { Injectable } from '@angular/core';
import { MatDatepickerIntl } from '@angular/material/datepicker';

/**
 * The datepicker labels in Portuguese. Provide it in each component that imports `MatDatepickerModule`,
 * since the module's own `MatDatepickerIntl` hides one provided at the root.
 */
@Injectable()
export class PtBrDatepickerIntl extends MatDatepickerIntl {
	override calendarLabel = 'Calendário';
	override openCalendarLabel = 'Abrir calendário';
	override closeCalendarLabel = 'Fechar calendário';
	override prevMonthLabel = 'Mês anterior';
	override nextMonthLabel = 'Próximo mês';
	override prevYearLabel = 'Ano anterior';
	override nextYearLabel = 'Próximo ano';
	override prevMultiYearLabel = '24 anos anteriores';
	override nextMultiYearLabel = 'Próximos 24 anos';
	override switchToMonthViewLabel = 'Escolher data';
	override switchToMultiYearViewLabel = 'Escolher mês e ano';
	override startDateLabel = 'Data inicial';
	override endDateLabel = 'Data final';
	override comparisonDateLabel = 'Período de comparação';

	override formatYearRangeLabel(start: string, end: string): string {
		return `de ${start} a ${end}`;
	}
}
