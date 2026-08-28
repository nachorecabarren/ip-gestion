import { ActivatedRoute, Router } from '@angular/router';

/**
 * Opens a "create" modal when the route was reached with the given query
 * param present (e.g. a Dashboard quick-action deep link), then strips the
 * param so a page refresh doesn't reopen it.
 */
export function openIfQueryParam(
  route: ActivatedRoute,
  router: Router,
  paramName: string,
  openFn: () => void,
) {
  if (route.snapshot.queryParamMap.get(paramName)) {
    openFn();
    router.navigate([], { queryParams: {}, replaceUrl: true });
  }
}
