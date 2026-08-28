import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { GUIDE_CATEGORIES, GUIDES, Guide } from './guides.data';

interface CategoryGroup {
  category: string;
  guides: Guide[];
}

@Component({
  selector: 'app-ayuda',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ayuda.component.html',
  styleUrls: ['./ayuda.component.scss'],
})
export class AyudaComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  search = signal('');
  selectedId = signal<string>(GUIDES[0].id);

  readonly categories = GUIDE_CATEGORIES;

  readonly filtered = computed<CategoryGroup[]>(() => {
    const q = this.search().trim().toLowerCase();
    const matches = (g: Guide) =>
      !q ||
      g.title.toLowerCase().includes(q) ||
      g.summary.toLowerCase().includes(q) ||
      g.category.toLowerCase().includes(q) ||
      g.steps.some((s) => s.title.toLowerCase().includes(q) || (s.detail ?? '').toLowerCase().includes(q));

    return this.categories
      .map((category) => ({ category, guides: GUIDES.filter((g) => g.category === category && matches(g)) }))
      .filter((group) => group.guides.length > 0);
  });

  readonly selected = computed<Guide | null>(() => GUIDES.find((g) => g.id === this.selectedId()) ?? null);

  readonly totalMatches = computed(() => this.filtered().reduce((sum, group) => sum + group.guides.length, 0));

  ngOnInit() {
    const requested = this.route.snapshot.queryParamMap.get('guia');
    if (requested && GUIDES.some((g) => g.id === requested)) {
      this.selectedId.set(requested);
    }
  }

  select(guide: Guide) {
    this.selectedId.set(guide.id);
    this.router.navigate([], { queryParams: {}, replaceUrl: true });
  }

  clearSearch() {
    this.search.set('');
  }
}
