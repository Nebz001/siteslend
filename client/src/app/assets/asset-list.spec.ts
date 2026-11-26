import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Asset, PagedResult } from './asset.model';
import { AssetList } from './asset-list';

const URL = '/api/assets?page=1&pageSize=20';

const assets: Asset[] = [
  { id: '1', assetTag: 'LAP-0001', categoryName: 'Laptops', description: '14-inch demo laptop', condition: 'Good', state: 'Available' },
  { id: '2', assetTag: 'LAP-0004', categoryName: 'Laptops', description: null, condition: 'Damaged', state: 'UnderRepair' },
];

const result = (items: Asset[]): PagedResult<Asset> => ({
  items,
  totalCount: items.length,
  page: 1,
  pageSize: 20,
});

describe('AssetList', () => {
  async function setup() {
    await TestBed.configureTestingModule({
      imports: [AssetList],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    const fixture = TestBed.createComponent(AssetList);
    const http = TestBed.inject(HttpTestingController);
    await fixture.whenStable();
    const page = () => fixture.nativeElement as HTMLElement;
    return { fixture, http, page };
  }

  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('shows a loading state while the request is in flight', async () => {
    const { http, page } = await setup();

    expect(page().querySelector('[role="status"]')?.textContent).toContain('Loading assets');
    expect(page().querySelector('table')).toBeNull();

    http.expectOne(URL).flush(result([]));
  });

  it('shows the assets in a table with headers, and as cards for small screens', async () => {
    const { fixture, http, page } = await setup();

    http.expectOne(URL).flush(result(assets));
    await fixture.whenStable();

    expect(page().querySelector('[role="status"]')).toBeNull();

    const headers = Array.from(page().querySelectorAll('thead th')).map((th) => th.textContent?.trim());
    expect(headers).toEqual(['Asset tag', 'Category', 'Description', 'Condition', 'State']);
    expect(page().querySelectorAll('thead th[scope="col"]').length).toBe(5);

    const rows = page().querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
    expect(rows[0].textContent).toContain('LAP-0001');
    expect(rows[0].textContent).toContain('14-inch demo laptop');
    expect(rows[1].textContent).toContain('No description');

    expect(page().querySelectorAll('ul li').length).toBe(2);
    expect(page().textContent).toContain('Showing 2 of 2 assets');
  });

  it('shows the state as text and an icon, not colour alone', async () => {
    const { fixture, http, page } = await setup();

    http.expectOne(URL).flush(result(assets));
    await fixture.whenStable();

    const badge = page().querySelectorAll('tbody tr')[1].querySelector('[data-slot="badge"]') as HTMLElement;
    expect(badge.textContent?.trim()).toBe('Under repair');
    expect(badge.querySelector('ng-icon')?.getAttribute('aria-hidden')).toBe('true');
    // The state colour wins over the badge's default text colour.
    expect(badge.classList).toContain('text-destructive');
    expect(badge.classList).not.toContain('text-foreground');
  });

  it('shows an empty state when there are no assets', async () => {
    const { fixture, http, page } = await setup();

    http.expectOne(URL).flush(result([]));
    await fixture.whenStable();

    expect(page().textContent).toContain('No assets yet');
    expect(page().querySelector('table')).toBeNull();
  });

  it('shows an error with a working Retry button when the API cannot be reached', async () => {
    const { fixture, http, page } = await setup();

    http.expectOne(URL).flush(null, { status: 500, statusText: 'Server Error' });
    await fixture.whenStable();

    const alert = page().querySelector('[role="alert"]');
    expect(alert?.textContent).toContain("Can't load the assets");
    expect(page().querySelector('table')).toBeNull();

    const retry = alert?.querySelector('button') as HTMLButtonElement;
    expect(retry.textContent?.trim()).toBe('Retry');
    retry.click();
    await fixture.whenStable();

    // Retry went back to loading and sent a second request.
    expect(page().querySelector('[role="alert"]')).toBeNull();
    http.expectOne(URL).flush(result(assets));
    await fixture.whenStable();
    expect(page().querySelectorAll('tbody tr').length).toBe(2);
  });
});
