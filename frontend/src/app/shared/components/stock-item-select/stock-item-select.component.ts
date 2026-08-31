import { Component, ElementRef, EventEmitter, HostListener, Input, Output, ViewChild, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { StockItem } from '../../models/models';
import { sortStockByModel } from '../../utils/stock-sort';

/**
 * Drop-in replacement for a native `<select>` bound to a stockItemId form control.
 * Native <option> can't render rich text (no bold), so this renders its own
 * popover list — which also lets us always show the color in bold and keep
 * the newest-first / Pro Max > Pro > Estándar ordering in one shared place.
 */
@Component({
  selector: 'app-stock-item-select',
  standalone: true,
  imports: [CommonModule],
  providers: [
    { provide: NG_VALUE_ACCESSOR, useExisting: StockItemSelectComponent, multi: true },
  ],
  styleUrl: './stock-item-select.component.scss',
  template: `
    <div class="stock-select" [class.stock-select--disabled]="disabled()">
      <button
        type="button"
        class="stock-select__trigger"
        (click)="toggle()"
        [disabled]="disabled()"
      >
        <span class="stock-select__trigger-label" *ngIf="selectedItem() as s; else placeholderLabel">
          {{ s.modelName }} {{ s.storageGb ? s.storageGb + 'GB' : '' }} <strong>{{ s.color }}</strong>
        </span>
        <ng-template #placeholderLabel>
          <span class="stock-select__placeholder">{{ placeholder }}</span>
        </ng-template>
        <svg class="stock-select__chevron" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="6 9 12 15 18 9"/></svg>
      </button>

      <div class="stock-select__panel" *ngIf="isOpen()">
        <input
          #searchInput
          type="text"
          class="stock-select__search"
          [placeholder]="searchPlaceholder"
          [value]="search()"
          (input)="search.set($any($event.target).value)"
        />

        <div class="stock-select__list">
          <button
            type="button"
            class="stock-select__option stock-select__option--empty"
            *ngIf="emptyOptionLabel"
            (click)="select(null)"
          >
            {{ emptyOptionLabel }}
          </button>

          <button
            type="button"
            class="stock-select__option"
            *ngFor="let s of filteredItems()"
            [class.stock-select__option--active]="s.id === selectedId()"
            (click)="select(s.id)"
          >
            <span class="stock-select__option-main">
              {{ s.modelName }} {{ s.storageGb ? s.storageGb + 'GB' : '' }} <strong>{{ s.color }}</strong>
            </span>
            <span class="stock-select__option-meta" *ngIf="s.imeiSerial || s.internalCode">
              ({{ s.imeiSerial || s.internalCode }})
            </span>
          </button>

          <div class="stock-select__empty" *ngIf="filteredItems().length === 0">
            Sin resultados.
          </div>
        </div>
      </div>
    </div>
  `,
})
export class StockItemSelectComponent implements ControlValueAccessor {
  private el = inject(ElementRef<HTMLElement>);
  @ViewChild('searchInput') searchInputRef?: ElementRef<HTMLInputElement>;

  @Input() placeholder = 'Seleccionar equipo...';
  @Input() searchPlaceholder = 'Buscar por modelo, color o IMEI...';
  @Input() emptyOptionLabel: string | null = null;

  /** Fires whenever the user actively picks (or clears) an item — useful for
   *  side-effects like auto-filling a price, on top of the plain CVA binding. */
  @Output() valueSelected = new EventEmitter<string | null>();

  private itemsSignal = signal<StockItem[]>([]);
  @Input() set items(v: StockItem[] | null) {
    this.itemsSignal.set(v ?? []);
  }

  isOpen = signal(false);
  search = signal('');
  selectedId = signal<string | null>(null);
  disabled = signal(false);

  private onChange: (value: string | null) => void = () => {};
  private onTouched: () => void = () => {};

  readonly sortedItems = computed(() => sortStockByModel(this.itemsSignal()));

  readonly filteredItems = computed(() => {
    const q = this.search().toLowerCase().trim();
    const list = this.sortedItems();
    if (!q) return list;
    return list.filter(s =>
      s.modelName.toLowerCase().includes(q) ||
      (s.color ?? '').toLowerCase().includes(q) ||
      (s.imeiSerial ?? '').toLowerCase().includes(q) ||
      (s.internalCode ?? '').toLowerCase().includes(q)
    );
  });

  readonly selectedItem = computed(() =>
    this.itemsSignal().find(s => s.id === this.selectedId()) ?? null
  );

  toggle() {
    if (this.disabled()) return;
    this.isOpen() ? this.close() : this.open();
  }

  open() {
    this.search.set('');
    this.isOpen.set(true);
    setTimeout(() => this.searchInputRef?.nativeElement.focus());
  }

  close() {
    this.isOpen.set(false);
    this.onTouched();
  }

  select(id: string | null) {
    this.selectedId.set(id);
    this.onChange(id);
    this.valueSelected.emit(id);
    this.close();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (this.isOpen() && !this.el.nativeElement.contains(event.target as Node)) {
      this.close();
    }
  }

  @HostListener('document:keydown.escape')
  onEscape() {
    if (this.isOpen()) this.close();
  }

  writeValue(value: string | null): void {
    this.selectedId.set(value);
  }
  registerOnChange(fn: (value: string | null) => void): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }
  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }
}
