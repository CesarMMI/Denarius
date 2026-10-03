import { httpResource } from '@angular/common/http';
import { computed, Injectable } from '@angular/core';

const URL = 'data/default-colors.json';

@Injectable({
	providedIn: 'root',
})
export class ColorsService {
	private readonly _resource = httpResource<string[]>(() => URL);

	readonly resource = this._resource.asReadonly();
	readonly colors = computed(() => (this.resource.hasValue() ? this.resource.value() : []));

	reload() {
		this._resource.reload();
	}
}
