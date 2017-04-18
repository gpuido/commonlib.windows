namespace OasCommonLib.Helpers
{
    using System.Collections.Generic;
    using System;
    using OasCommonLib.Constants;

    public sealed class FieldEnumerator
    {
        public static readonly Dictionary<int, string> _indecies;
        public static readonly Dictionary<string, int> _fields;

        static FieldEnumerator()
        {
            _indecies = new Dictionary<int, string>
            {
                // envelope
                { 1, "envelope_id" },
                { 2, "envelope_name" },
                { 3, "est_system" },
                { 4, "software_version" },
                { 5, "database_version" },
                { 6, "database_date" },
                { 7, "unique_id" },
                { 8, "ro_id" },
                { 9, "estimate_file_id" },
                { 10, "support_number" },
                { 11, "estimate_country" },
                { 12, "top_secret" },
                { 13, "h_trans_id" },
                { 14, "h_ctrl_no" },
                { 15, "trans_type" },
                { 16, "status" },
                { 17, "create_date" },
                { 18, "transmit_date" },
                { 19, "includes_admin" },
                { 20, "includes_vehicle" },
                { 21, "includes_estimate" },
                { 22, "includes_profile" },
                { 23, "includes_total" },
                { 24, "includes_vendor" },
                { 25, "ems_version" },
                { 26, "owner_user_id" },
                { 27, "company_id" },
                { 28, "ins_grp_id" },
                { 29, "mcf" },
                { 30, "arrival_type" },
                { 31, "claim_number" },
                { 32, "manually_added" },
                { 33, "vehicle_action" },
                { 34, "has_add_info" },
                { 35, "has_precond_info" },
                { 36, "has_audio_note" },
                { 37, "has_sup_info" },

                // vehicle
                { 100, "impact1" },
                { 101, "impact2" },
                { 102, "dmg_memo" },
                { 103, "db_v_code" },
                { 104, "plate_number" },
                { 105, "plate_state" },
                { 106, "vehicle_vin" },
                { 107, "vehicle_condition" },
                { 108, "vehicle_production" },
                { 109, "vehicle_production_year" },
                { 110, "vehicle_makecode" },
                { 111, "vehicle_make_desc" },
                { 112, "vehicle_model" },
                { 113, "vehicle_type" },
                { 114, "vehicle_style" },
                { 115, "vehicle_trim_code" },
                { 116, "vehicle_trim_color" },
                { 117, "vehicle_mvdg_code" },
                { 118, "vehicle_engine" },
                { 119, "vehicle_milage" },
                { 120, "vehicle_color" },
                { 121, "vehicle_tone" },
                { 122, "vehicle_stage" },
                { 123, "paint_cd1" },
                { 124, "paint_cd2" },
                { 125, "paint_cd3" },
                { 126, "vehicle_memo" },

                //details
                { 200, "line_number" },
                { 201, "line_indicator" },
                { 202, "line_reference" },
                { 203, "db_reference" },
                { 204, "orig_db_reference" },
                { 205, "unique_sequence" },
                { 206, "line_description" },
                { 207, "part_type" },
                { 208, "glass_flag" },
                { 209, "oem_part_number" },
                { 210, "price_included" },
                { 211, "alt_part_i" },
                { 212, "tax_part" },
                { 213, "db_price" },
                { 214, "act_price" },
                { 215, "price_j" },
                { 216, "certified_part" },
                { 217, "part_quantity" },
                { 218, "alt_part_number" },
                { 219, "alt_override" },
                { 220, "alt_part_m" },
                { 221, "db_hours" },
                { 222, "mod_lb_hours" },
                { 223, "labor_included" },
                { 224, "labor_operation" },
                { 225, "labor_hours_j" },
                { 226, "labor_type_j" },
                { 227, "labor_operation_j" },
                { 228, "paint_stage" },
                { 229, "paint_tone" },
                { 231, "labor_tax" },
                { 232, "labor_amount" },
                { 233, "misc_amount" },
                { 234, "misc_sublt" },
                { 235, "misc_tax" },

                // add info
                { 300, "id" },
                { 301, "reference" },
                { 302, "file_name" },
                { 303, "note" },
                { 304, "updated" },
                { 305, "tz" },
                { 306, "proof" },
                { 307, "user_id" }
            };
            _fields = new Dictionary<string, int>();
            foreach(var ind in _indecies)
            {
                _fields.Add(ind.Value, ind.Key);
            }
        }

        public static string FindIndex(string fieldName)
        {
            if (_fields.ContainsKey(fieldName))
            {
                return "%" + _fields[fieldName];
            }

            return fieldName;
        }

        public static string FindField(string index)
        {
            if (int.TryParse((index ?? OasStringConstants.Space).Substring(1), out int idx) && idx > 0)
            {
                return _indecies[idx];
            }

            return String.Empty;
        }
    }
}
