import { PtBrDatepickerIntl } from './pt-br-datepicker-intl';

describe('PtBrDatepickerIntl', () => {
	const intl = new PtBrDatepickerIntl();

	it.each<[keyof PtBrDatepickerIntl, string]>([
		['calendarLabel', 'Calendário'],
		['openCalendarLabel', 'Abrir calendário'],
		['closeCalendarLabel', 'Fechar calendário'],
		['prevMonthLabel', 'Mês anterior'],
		['nextMonthLabel', 'Próximo mês'],
		['prevYearLabel', 'Ano anterior'],
		['nextYearLabel', 'Próximo ano'],
		['prevMultiYearLabel', '24 anos anteriores'],
		['nextMultiYearLabel', 'Próximos 24 anos'],
		['switchToMonthViewLabel', 'Escolher data'],
		['switchToMultiYearViewLabel', 'Escolher mês e ano'],
		['startDateLabel', 'Data inicial'],
		['endDateLabel', 'Data final'],
		['comparisonDateLabel', 'Período de comparação'],
	])('should name %s in Portuguese', (label, text) => {
		expect(intl[label]).toBe(text);
	});

	it('should describe a range of years in Portuguese', () => {
		expect(intl.formatYearRangeLabel('2016', '2039')).toBe('de 2016 a 2039');
	});

	it('should show a range of years with a dash', () => {
		expect(intl.formatYearRange('2016', '2039')).toBe('2016 – 2039');
	});
});
