import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { Parties } from './parties';
import { Party } from './party.models';
import { PartyService } from './party.service';

describe('Parties', () => {
  const getAll = vi.fn();
  const registerIndividual = vi.fn();
  const registerOrganization = vi.fn();

  const created: Party = {
    id: 1,
    partyType: 'Individual',
    name: 'Ahmed Ali',
    email: 'ahmed@test.com',
    phone: '0500000000',
    nationalId: '1012345678',
    registrationNumber: null,
    contactPerson: null,
  };

  beforeEach(() => {
    getAll.mockReset();
    getAll.mockReturnValue(of([]));
    registerIndividual.mockReset();
    registerOrganization.mockReset();
    TestBed.configureTestingModule({
      imports: [Parties],
      providers: [
        {
          provide: PartyService,
          useValue: { getAll, registerIndividual, registerOrganization },
        },
      ],
    });
  });

  it('does not call the API when the individual form is invalid', () => {
    const component = TestBed.createComponent(Parties).componentInstance;

    component.submit();

    expect(registerIndividual).not.toHaveBeenCalled();
    expect(component.individualForm.touched).toBe(true);
  });

  it('rejects a National ID that is not exactly 10 digits', () => {
    const component = TestBed.createComponent(Parties).componentInstance;

    component.individualForm.controls.nationalId.setValue('12345');

    expect(component.individualForm.controls.nationalId.hasError('pattern')).toBe(true);
  });

  it('sends trimmed values and reloads the list from the API', () => {
    getAll.mockReturnValueOnce(of([])).mockReturnValueOnce(of([created]));
    registerIndividual.mockReturnValue(of(created));
    const component = TestBed.createComponent(Parties).componentInstance;
    component.individualForm.setValue({
      name: '  Ahmed Ali ',
      nationalId: '1012345678',
      email: 'ahmed@test.com',
      phone: '0500000000',
    });

    component.submit();

    expect(registerIndividual).toHaveBeenCalledWith({
      name: 'Ahmed Ali',
      nationalId: '1012345678',
      email: 'ahmed@test.com',
      phone: '0500000000',
    });
    expect(getAll).toHaveBeenCalledTimes(2);
    expect(component.parties()).toEqual([created]);
  });

  it('does not call the organization API when the registration number is invalid', () => {
    const component = TestBed.createComponent(Parties).componentInstance;
    component.setKind('organization');
    component.organizationForm.patchValue({ registrationNumber: 'ABC' });

    component.submit();

    expect(registerOrganization).not.toHaveBeenCalled();
  });
});