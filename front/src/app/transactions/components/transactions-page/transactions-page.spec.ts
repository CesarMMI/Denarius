import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { TransactionsPage } from './transactions-page';

describe('TransactionsPage', () => {
	let component: TransactionsPage;
	let fixture: ComponentFixture<TransactionsPage>;

	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [TransactionsPage],
			providers: [{ provide: ActivatedRoute, useValue: { snapshot: { queryParamMap: convertToParamMap({}) } } }],
		}).compileComponents();

		fixture = TestBed.createComponent(TransactionsPage);
		component = fixture.componentInstance;
		await fixture.whenStable();
	});

	it('should create', () => {
		expect(component).toBeTruthy();
	});
});
