import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Home } from './home';

describe('Home', () => {
  async function setup() {
    await TestBed.configureTestingModule({
      imports: [Home],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    const fixture = TestBed.createComponent(Home);
    const http = TestBed.inject(HttpTestingController);
    await fixture.whenStable();
    const page = () => fixture.nativeElement as HTMLElement;
    return { fixture, http, page };
  }

  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('shows a loading state, then the result of /api/health', async () => {
    const { fixture, http, page } = await setup();

    // Request is in flight: loading state, no result yet.
    expect(page().querySelector('[role="status"]')?.textContent).toContain('Checking system status');
    expect(page().querySelector('dl')).toBeNull();

    http.expectOne('/api/health').flush({ status: 'ok', database: 'up' });
    await fixture.whenStable();

    expect(page().querySelector('[role="status"]')).toBeNull();
    expect(page().querySelector('dl')?.textContent).toContain('Running');
    expect(page().querySelector('dl')?.textContent).toContain('Connected');
  });

  it('shows the database as not connected when the API reports it down', async () => {
    const { fixture, http, page } = await setup();

    http.expectOne('/api/health').flush({ status: 'ok', database: 'down' });
    await fixture.whenStable();

    expect(page().querySelector('dl')?.textContent).toContain('Not connected');
    expect(page().textContent).toContain("can't reach PostgreSQL");
  });

  it('shows an error with a working retry button when the API cannot be reached', async () => {
    const { fixture, http, page } = await setup();

    http.expectOne('/api/health').flush(null, { status: 500, statusText: 'Server Error' });
    await fixture.whenStable();

    const alert = page().querySelector('[role="alert"]');
    expect(alert?.textContent).toContain("Can't reach the API");

    const retry = alert?.querySelector('button') as HTMLButtonElement;
    expect(retry.dataset['slot']).toBe('button');
    expect(retry.textContent?.trim()).toBe('Try again');
    retry.click();
    await fixture.whenStable();

    // Retry went back to loading and sent a second request.
    expect(page().querySelector('[role="alert"]')).toBeNull();
    http.expectOne('/api/health').flush({ status: 'ok', database: 'up' });
  });
});
