'use client'

import FormInput from '@/_components/FormInput'
import FormLabel from '@/_components/FormLabel'  
import { useState } from 'react'
import { IAddressData } from '../model'
import TypeAheadCombobox  from '../../_components/TypeAheadCombobox'
import Select from '@/_components/Select'


interface IEhakDto {
  aadresstekst: string,
  kort_nr: string,
  asustusyksus: string,
  omavalitus: string,
  maakond: string,
  sihtnumber: string,
}

interface ICountryName{
  name:string
}
export default function ClientAddress({
  name,
  address
}: {
  name: string,
  address?:IAddressData |undefined
}) {
  
  const ct = require("countries-and-timezones");  // eslint-disable-line  @typescript-eslint/no-require-imports
  const countries = Object.values(ct.getAllCountries() as ICountryName[]).map((x) => x.name);
  const [selectedAddress, setSelectedAddress] = useState<IAddressData | null>(!address?null:address);
  //const [addressData, setAddressData] = useState<IAddressData[]>([]);
  const MYCOUNTRY = 'Estonia';
  const [country, setCountry] = useState(!address?MYCOUNTRY:address.country);
  const useEhak = country===MYCOUNTRY;

    
  const queryRemoteAddressData = (inputValue: string) =>
    new Promise<IAddressData[]>((resolve) => {
      if (!inputValue || inputValue.length < 3) {
        resolve([]);
        return;
      }
      const url =
        "https://inaadress.maaamet.ee/inaadress/gazetteer?" +
        new URLSearchParams({
          results: '20',
          appartment: '1',
          unik: '0',
          address: inputValue,
        }).toString();

      fetch(url)
        .then((response) => response.json())
        .then((json) => {
          if (!json.addresses) return [];
          const options = json.addresses.map((addr: IEhakDto) => {
            const address = {
              street: addr.aadresstekst + (!addr.kort_nr ? "" : "-" + addr.kort_nr).trim(),
              city: addr.asustusyksus,
              region: [
                addr.omavalitus,
                addr.maakond,
              ]
                .filter((item) => item)
                .join(", "),
              postalCode: addr.sihtnumber
            } 

            return address;
          });
          resolve(options);
        })
        .catch((error) => {
          console.error(error);
        });
    });

  
  return (
    <> 
      <div className="sm:col-span-3  sm:col-start-1"> 
        <FormLabel name='country' label='Country'></FormLabel>
        <div className="mt-2 grid grid-cols-1">
          <Select  
            id="country"
            name="country"
            defaultValue={country}
            onChange={(e)=>{
              setCountry(e.currentTarget.value);
            }}>
              <option value=''>-</option>
            {countries.map((country) => {
              
              return (
                <option key={country} value={country}>{country}</option>
              )
            })}
          </Select>  
        </div>
      </div>
      <div className="col-span-full">
      <FormLabel name='street' label=' Street address'></FormLabel>
      {useEhak ? (
        <TypeAheadCombobox
           name={name}

           placeholder='Estonian address ...'
           defaultValue={selectedAddress}
           onSearch={(event,datasourceTarget) => {
            queryRemoteAddressData(event.currentTarget.value)
              .then((result) => {
                datasourceTarget(result);
            })
           }}
           onItemChange={(item)=>{
              setSelectedAddress(item);
           }}
           displayFormatter={(address) => {
            const ehak = address as IAddressData
            return ehak?.street;
           }}
           optionFormatter={(address)=>{
              return [
                address.street,
                address.city,
                address.region,
                address.postalCode,
              ]
                .filter((item) => item)
                .join(", ")
           }}
          >

        </TypeAheadCombobox>
      ) : (
        <FormInput name={`${name}[street]`} defaultValue={selectedAddress?.street} ></FormInput>
      )}
      </div>
    
      <div className="sm:col-span-2 sm:col-start-1">
        <FormInput name='city' label='City' defaultValue={selectedAddress?.city} ></FormInput>
      </div>

      <div className="sm:col-span-2">
        <FormInput name='region' label='State / Province' defaultValue={selectedAddress?.region} ></FormInput>
      </div>

      <div className="sm:col-span-2">
      <FormInput name='postal-code' label='ZIP / Postal code' defaultValue={selectedAddress?.postalCode} ></FormInput>
      </div></>
  )
}
