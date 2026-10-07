import { effect, ResourceStatus, Signal, untracked } from '@angular/core';

/** The part of a resource (an `httpResource`, for example) that `reloadWhenIdle` uses. */
export interface ReloadableResource {
	readonly status: Signal<ResourceStatus>;
	reload(): boolean;
}

/**
 * Returns a `reload` for the resource that is not lost while it is loading. Angular ignores `reload()` during a load
 * (the first one, a change of params or another reload), so a list answered before a change the API accepted would
 * stay stale. Here the reload is kept pending and sent once the load in progress succeeds; if that load fails, it is
 * dropped, and the user's "Recarregar" remains the way out. Must be called in an injection context.
 */
export function reloadWhenIdle(resource: ReloadableResource): () => void {
	let pending = false;

	effect(() => {
		const status = resource.status();
		if (!pending || status === 'loading' || status === 'reloading') return;
		// Cleared first: the reload below is not marked again, so nothing loops.
		pending = false;
		if (status === 'resolved') untracked(() => resource.reload());
	});

	return () => {
		if (!resource.reload()) pending = true;
	};
}
