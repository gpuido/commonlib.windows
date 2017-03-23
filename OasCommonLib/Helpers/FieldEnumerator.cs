
using System.Collections.Generic;

namespace OasCommonLib.Helpers
{
    public sealed class FieldEnumerator
    {
        public static readonly Dictionary<int, string> _indecies;
        public static readonly Dictionary<string, int> _fields;

        static FieldEnumerator()
        {
            _indecies = new Dictionary<int, string>();

            // envelope
            _indecies.Add(1, "envelope_id");
            _indecies.Add(2, "envelope_name");
            _indecies.Add(3, "est_system");
            _indecies.Add(4, "software_version");
            _indecies.Add(5, "database_version");
            _indecies.Add(6, "database_date");
            _indecies.Add(7, "unique_id");
            _indecies.Add(8, "ro_id");
            _indecies.Add(9, "estimate_file_id");
            _indecies.Add(10, "support_number");
            _indecies.Add(11, "estimate_country");
            _indecies.Add(12, "top_secret");
            _indecies.Add(13, "h_trans_id");
            _indecies.Add(14, "h_ctrl_no");
            _indecies.Add(15, "trans_type");
            _indecies.Add(16, "status");
            _indecies.Add(17, "create_date");
            _indecies.Add(18, "transmit_date");
            _indecies.Add(19, "includes_admin");
            _indecies.Add(20, "includes_vehicle");
            _indecies.Add(21, "includes_estimate");
            _indecies.Add(22, "includes_profile");
            _indecies.Add(23, "includes_total");
            _indecies.Add(24, "includes_vendor");
            _indecies.Add(25, "ems_version");
            _indecies.Add(26, "owner_user_id");
            _indecies.Add(27, "company_id");
            _indecies.Add(28, "ins_grp_id");
            _indecies.Add(29, "mcf");
            _indecies.Add(30, "arrival_type");
            _indecies.Add(31, "claim_number");
            _indecies.Add(32, "manually_added");
            _indecies.Add(33, "vehicle_action");
            _indecies.Add(34, "has_add_info");
            _indecies.Add(35, "has_precond_info");
            _indecies.Add(36, "has_audio_note");
            _indecies.Add(37, "has_sup_info");

            // vehicle
            _indecies.Add(100, "impact1");
            _indecies.Add(101, "impact2");
            _indecies.Add(102, "dmg_memo");
            _indecies.Add(103, "db_v_code");
            _indecies.Add(104, "plate_number");
            _indecies.Add(105, "plate_state");
            _indecies.Add(106, "vehicle_vin");
            _indecies.Add(107, "vehicle_condition");
            _indecies.Add(108, "vehicle_production");
            _indecies.Add(109, "vehicle_production_year");
            _indecies.Add(110, "vehicle_makecode");
            _indecies.Add(111, "vehicle_make_desc");
            _indecies.Add(112, "vehicle_model");
            _indecies.Add(113, "vehicle_type");
            _indecies.Add(114, "vehicle_style");
            _indecies.Add(115, "vehicle_trim_code");
            _indecies.Add(116, "vehicle_trim_color");
            _indecies.Add(117, "vehicle_mvdg_code");
            _indecies.Add(118, "vehicle_engine");
            _indecies.Add(119, "vehicle_milage");
            _indecies.Add(120, "vehicle_color");
            _indecies.Add(121, "vehicle_tone");
            _indecies.Add(122, "vehicle_stage");
            _indecies.Add(123, "paint_cd1");
            _indecies.Add(124, "paint_cd2");
            _indecies.Add(125, "paint_cd3");
            _indecies.Add(126, "vehicle_memo");

            //details
            _indecies.Add(200, "line_number");
            _indecies.Add(201, "line_indicator");
            _indecies.Add(202, "line_reference");
            _indecies.Add(203, "db_reference");
            _indecies.Add(204, "orig_db_reference");
            _indecies.Add(205, "unique_sequence");
            _indecies.Add(206, "line_description");
            _indecies.Add(207, "part_type");
            _indecies.Add(208, "glass_flag");
            _indecies.Add(209, "oem_part_number");
            _indecies.Add(210, "price_included");
            _indecies.Add(211, "alt_part_i");
            _indecies.Add(212, "tax_part");
            _indecies.Add(213, "db_price");
            _indecies.Add(214, "act_price");
            _indecies.Add(215, "price_j");
            _indecies.Add(216, "certified_part");
            _indecies.Add(217, "part_quantity");
            _indecies.Add(218, "alt_part_number");
            _indecies.Add(219, "alt_override");
            _indecies.Add(220, "alt_part_m");
            _indecies.Add(221, "db_hours");
            _indecies.Add(222, "mod_lb_hours");
            _indecies.Add(223, "labor_included");
            _indecies.Add(224, "labor_operation");
            _indecies.Add(225, "labor_hours_j");
            _indecies.Add(226, "labor_type_j");
            _indecies.Add(227, "labor_operation_j");
            _indecies.Add(228, "paint_stage");
            _indecies.Add(229, "paint_tone");
            _indecies.Add(231, "labor_tax");
            _indecies.Add(232, "labor_amount");
            _indecies.Add(233, "misc_amount");
            _indecies.Add(234, "misc_sublt");
            _indecies.Add(235, "misc_tax");

            // add info
            _indecies.Add(300, "id");
            _indecies.Add(301, "reference");
            _indecies.Add(302, "file_name");
            _indecies.Add(303, "note");
            _indecies.Add(304, "updated");
            _indecies.Add(305, "tz");
            _indecies.Add(306, "proof");
            _indecies.Add(307, "user_id");

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
            int idx;

            if (int.TryParse((index ?? " ").Substring(1), out idx) && idx > 0)
            {
                return _indecies[idx];
            }

            return string.Empty;
        }
    }
}
