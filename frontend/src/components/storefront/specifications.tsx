import { Card, CardContent } from "@/components/ui/card";

interface SpecificationItem {
  label: string;
  value: string;
}

interface SpecificationsProps {
  sku?: string;
  brand?: string;
  category?: string;
  description?: string;
}

export function Specifications({ sku, brand, category }: SpecificationsProps) {
  const specs: SpecificationItem[] = [];

  if (brand) specs.push({ label: "Brand", value: brand });
  if (sku) specs.push({ label: "SKU", value: sku });
  if (category) specs.push({ label: "Category", value: category });
  specs.push({ label: "Country of Origin", value: "India" });

  if (specs.length === 0) return null;

  return (
    <Card>
      <CardContent className="p-0">
        <table className="w-full text-sm" role="table">
          <tbody>
            {specs.map((spec, index) => (
              <tr
                key={spec.label}
                className={index % 2 === 0 ? "bg-muted/50" : ""}
              >
                <td className="px-4 py-3 font-medium text-muted-foreground w-1/3">{spec.label}</td>
                <td className="px-4 py-3">{spec.value}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </CardContent>
    </Card>
  );
}
